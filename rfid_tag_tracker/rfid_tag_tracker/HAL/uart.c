/*
 * uart.c
 *
 * Created: 8/14/2017 2:30:43 PM
 *  Author: warre
 */ 
#include "uart.h"
#include "isr.h"

// local constants
#define UART_LOWEST_BAUD_TIMEOUT_MICROS 8333

// macro functions
#define UART_RST_PERIPHERAL {\
UCSR0A = 0;\
UCSR0B = 0;\
UCSR0C = 0;\
core_atomic16BitSFRWrite(&UBRR0H, &UBRR0L, 0);\
}

#define UART_ENABLE_PERIPHERAL (UCSR0B |= ((1 << TXEN0) | (1 << RXEN0)))
#define UART_DISABLE_PERIPHERAL (UCSR0B &= ~((1 << TXEN0) | (1 << RXEN0)))

#define UART_SET_MULTI_PROCESSOR_MODE (UCSR0A |= (1 << MPCM0))
#define UART_SET_ASYNC_OPERATION (UCSR0C &= ~((1 << UMSEL00) | (1 << UMSEL01)))

#define UART_SET_NO_PARITY (UCSR0C &= ~((1 << UPM00) | (1 << UPM01)))
#define UART_SET_EVEN_PARITY {\
UART_SET_NO_PARITY;\
UCSR0C |= (1 << UPM01);\
}
#define UART_SET_ODD_PARITY (UCSR0C |= ((1 << UPM00) | (1 << UPM01)))

#define UART_SET_1_STOP_BIT (UCSR0C &= ~(1 << USBS0))
#define UART_SET_2_STOP_BITS (UCSR0C |= (1 << USBS0))

#define UART_CLR_DATA_SIZE_BITS {\
UCSR0B &= ~(1 << UCSZ02);\
UCSR0C &= ~((1 << UCSZ01) | (1 << UCSZ00));\
}

#define UART_SET_BAUD_2X (UCSR0A |= (1 << U2X0))
#define UART_CLR_BAUD_2X (UCSR0A &= ~(1 << U2X0))

#define UART_IS_TX_REG_EMPTY (UCSR0A & (1 << UDRE0))
#define UART_IS_TX_COMPLETE (UCSR0A & (1 << TXC0))
#define UART_CLR_TX_COMPLETE_FLAG (UCSR0A |= (1 << TXC0))

#define UART_IS_RX_DATA_AVAIL (UCSR0A & (1 << RXC0))

#define UART_WRITE_DATA(byte) (UDR0 = byte)
#define UART_READ_DATA(byte) (byte = UDR0)

#define UART_CHECK_ERROR_FLAGS (UCSR0A & 0b11100)

#define UART_ENABLE_RX_INT (UCSR0B |= (1 << RXCIE0))
#define UART_DISABLE_RX_INT (UCSR0B &= ~(1 << RXCIE0))

#define UART_ENABLE_DATA_REG_EMPTY_INT (UCSR0B |= (1 << UDRIE0))
#define UART_DISABLE_DATA_REG_EMPTY_INT (UCSR0B &= ~(1 << UDRIE0))

// file specific global variables
static uint32_t gCurrentTimeOutInMicros = 0;
static bool_t gPeripheralInitialized = FALSE;
static uint8_t *gAsyncRxDataBuff = NULL;
static uint16_t gAsyncRxDataBuffLen, gAsyncRxDataBuffHead, gAsyncRxDataBuffTail;
static uart_error_t gAsyncRxError = uart_no_error;
static uint8_t *gAsyncTxDataBuff = NULL;
static uint16_t gAsyncTxDataBuffLen, gAsyncTxDataBuffHead, gAsyncTxDataBuffTail;
static uart_error_t gAsyncTxError = uart_no_error;

// locally defined functions
void uart_setNumDataBits(uint8_t numDataBits)
{
	UART_CLR_DATA_SIZE_BITS;
	
	switch (numDataBits)
	{
		case 6:
			UCSR0C |= (1 << UCSZ00);
			break;
		case 7:
			UCSR0C |= (1 << UCSZ01);
			break;
		case 8: 
			UCSR0C |= ((1 << UCSZ01) | (1 << UCSZ00));
			break;
		case 9:
			UCSR0B |= (1 << UCSZ02);
			UCSR0C |= ((1 << UCSZ01) | (1 << UCSZ00));
			break;
	}	
}

uart_error_t uart_setBaudRateBits(uart_baud_rates_t baud)
{
	uint8_t preScaler = 8;
	uint32_t baudDivider = 0;
	float_t tmpBaudDivider = 0;
	
	// always go with higher resolution if we can
	UART_SET_BAUD_2X;
	tmpBaudDivider = ((float)F_CPU / (float)(preScaler * baud)) - 1.0;
	baudDivider = core_roundToNearestInt(tmpBaudDivider);
	
	if (baudDivider > 4095)
	{
		preScaler = 16;
		UART_CLR_BAUD_2X;
		tmpBaudDivider = ((float)F_CPU / (float)(preScaler * baud)) - 1.0;
		baudDivider = (uint32_t)tmpBaudDivider;
		
		if (baudDivider > 4095)
			return uart_requested_baud_out_of_range;
	}
	
	core_atomic16BitSFRWrite(&UBRR0H, &UBRR0L, (uint16_t)(baudDivider & 0xffff));
	
	gCurrentTimeOutInMicros = (uint32_t)(((1.0 / (float)baud) * 1000000.0) * 20.0);	// 20 bits worth of time
	if (gCurrentTimeOutInMicros > UART_LOWEST_BAUD_TIMEOUT_MICROS)
		gCurrentTimeOutInMicros = UART_LOWEST_BAUD_TIMEOUT_MICROS;
	
	return uart_no_error;
}

uart_error_t uart_resolveError(uint8_t bits)
{
	uart_error_t errorCode = uart_unkown_error;
	uint8_t dummyByte;
	
	if (bits & (1 << FE0))
	{	
		errorCode = uart_frame_error;
		UART_READ_DATA(dummyByte);		
	}
	else if (bits & (1 << DOR0))
	{
		errorCode = uart_data_overrun;
		UART_READ_DATA(dummyByte);
	}
	else if (bits & (1 << UPE0))
	{	
		errorCode = uart_parity_error;
		UART_READ_DATA(dummyByte);
	}
	
	UART_CLR_TX_COMPLETE_FLAG;
	
	return errorCode;
}

uart_error_t uart_blockingWriteByte(uint8_t byte)
{
	uint32_t i;
	
	for (i = 0; i < gCurrentTimeOutInMicros; i++)
	{
		if (UART_IS_TX_REG_EMPTY)
		{
			UART_WRITE_DATA(byte);
			break;
		}
		
		if (UART_CHECK_ERROR_FLAGS)
			return uart_resolveError(UART_CHECK_ERROR_FLAGS);
		
		DELAY_US_CONST(1);			
	}
	
	for (i = 0; i < gCurrentTimeOutInMicros; i++)
	{
		if (UART_IS_TX_COMPLETE)
			return uart_no_error;
			
		DELAY_US_CONST(1);	
	}
	
	return uart_transaction_timeout;
}

uart_error_t uart_blockingReadByte(uint8_t *byte)
{
	uint32_t i;
	
	for (i = 0; i < gCurrentTimeOutInMicros; i++)
	{
		if (UART_IS_RX_DATA_AVAIL)
		{
			UART_READ_DATA(*byte);
			return uart_no_error;
		}
		
		if (UART_CHECK_ERROR_FLAGS)
			return uart_resolveError(UART_CHECK_ERROR_FLAGS);
		
		DELAY_US_CONST(1);
	}
	
	return uart_transaction_timeout;
}

void uart_asyncRxHandler()
{
    uint16_t head;
    
	head = gAsyncRxDataBuffHead % gAsyncRxDataBuffLen;
	UART_READ_DATA(gAsyncRxDataBuff[head]);
	gAsyncRxDataBuffHead++;
	gAsyncRxError = uart_no_error;
		
	if (UART_CHECK_ERROR_FLAGS)
		gAsyncRxError = uart_resolveError(UART_CHECK_ERROR_FLAGS);
		
}

void uart_asyncTxHandler()
{
	uint16_t tail;
	
	tail = gAsyncTxDataBuffTail % gAsyncTxDataBuffLen;
	UART_WRITE_DATA(gAsyncTxDataBuff[tail]);
	gAsyncTxDataBuffTail++;
	
	if (UART_CHECK_ERROR_FLAGS)
		gAsyncTxError = uart_resolveError(UART_CHECK_ERROR_FLAGS);
	
	if (gAsyncTxDataBuffTail == gAsyncTxDataBuffHead)
		UART_DISABLE_DATA_REG_EMPTY_INT;
}

// globally defined functions
uart_error_t uart_configurePeripheral(uart_params_t *params)
{
	uart_error_t errorCode;
	
	gPeripheralInitialized = FALSE;
	UART_RST_PERIPHERAL;
	UART_SET_ASYNC_OPERATION;
	
	if (params->multiProcessorMode)
		UART_SET_MULTI_PROCESSOR_MODE;
	
	switch (params->numStopBits)
	{
		case 1:
			UART_SET_1_STOP_BIT;
			break;
		case 2:
			UART_SET_2_STOP_BITS;
			break;
		default:
			return uart_illegal_num_stop_bits;
	}
	
	switch (params->parity)
	{
		case no_parity:
			UART_SET_NO_PARITY;
			break;
		case odd_parity:
			UART_SET_ODD_PARITY;
			break;
		case even_parity:
			UART_SET_EVEN_PARITY;
			break;
	}
	
	if ((params->numDataBits > 9) || (params->numDataBits < 5))
		return uart_illegal_num_data_bits;
	else
		uart_setNumDataBits(params->numDataBits);
		
	errorCode = uart_setBaudRateBits(params->baudRate);
	if (errorCode != uart_no_error)
		return errorCode;
	
	UART_ENABLE_PERIPHERAL;
	gPeripheralInitialized = TRUE;
	
	return uart_no_error;
}

void uart_deConfigurePeripheral()
{
	UART_RST_PERIPHERAL;
	
	// pull down async rx 
    gAsyncRxDataBuff = NULL;
    gAsyncRxDataBuffLen = gAsyncRxDataBuffHead = gAsyncRxDataBuffTail = 0;
	
	// pull down async tx
	gAsyncTxDataBuff = NULL;
	gAsyncTxDataBuffLen = gAsyncTxDataBuffHead = gAsyncTxDataBuffTail = 0;
	
	// pull down general config
	gCurrentTimeOutInMicros = 0;
	gPeripheralInitialized = FALSE;
	
	// remove ISRs
	isr_unRegisterHandlerFunc(USART_RX_vect_num);
	isr_unRegisterHandlerFunc(USART_UDRE_vect_num);
}

uart_error_t uart_configureAsyncRx(uint8_t *internalRxDataBuff, uint16_t internalRxDataBuffLen)
{
    uint16_t i;

	if (!gPeripheralInitialized)
		return uart_peripheral_uninitialized;
	if (internalRxDataBuff == NULL)
		return uart_attempted_to_dereference_null_ptr;
		
	gAsyncRxError = uart_no_error;
    gAsyncRxDataBuff = internalRxDataBuff;
	
	// for speed purposes, the data buffer length MUST be a power of two
	// if it is not, this force loop will force it to become a power of two
    for (i = 0; i < 16; i++)
    {
        if ((internalRxDataBuffLen >> i) & 0x1)
        {
            gAsyncRxDataBuffLen = (0x1 << i);
            break;
        }
    }    
    
	gAsyncRxDataBuffHead = gAsyncRxDataBuffTail = 0;
	isr_registerHandlerFunc(USART_RX_vect_num, uart_asyncRxHandler);
	UART_ENABLE_RX_INT;
	
	return uart_no_error;
}

uart_error_t uart_configureAsyncTx(uint8_t *internalTxDataBuff, uint16_t internalTxDataBuffLen)
{
    uint16_t i;

    if (!gPeripheralInitialized)
		return uart_peripheral_uninitialized;
    if (internalTxDataBuff == NULL)
		return uart_attempted_to_dereference_null_ptr;
    
    gAsyncTxError = uart_no_error;
    gAsyncTxDataBuff = internalTxDataBuff;
    
    // for speed purposes, the data buffer length MUST be a power of two
    // if it is not, this force loop will force it to become a power of two
    for (i = 0; i < 16; i++)
    {
	    if ((internalTxDataBuffLen >> i) & 0x1)
	    {
		    gAsyncTxDataBuffLen = (0x1 << i);
		    break;
	    }
    }
    
    gAsyncTxDataBuffHead = gAsyncTxDataBuffTail = 0;
    isr_registerHandlerFunc(USART_UDRE_vect_num, uart_asyncTxHandler);
    
    return uart_no_error;
}

uart_error_t uart_blockingWrite(uart_transaction_t *transac)
{
	uint16_t i;
	uart_error_t errorCode = uart_no_error;
	
	if (!gPeripheralInitialized)
		return uart_peripheral_uninitialized;
		
	core_enterBlockingTransaction();
	
	if ((transac->data == NULL) && (transac->len > 0))
	{
		errorCode = uart_attempted_to_dereference_null_ptr;
		goto UART_BLOCKING_WRITE_CLEANUP;
	}
	
	for (i = 0; i < transac->len; i++)
	{
		errorCode = uart_blockingWriteByte(transac->data[i]);
		if (errorCode != uart_no_error)
			goto UART_BLOCKING_WRITE_CLEANUP;
	}
	
	if ((transac->useCb) && (transac->cb == NULL))
		errorCode = uart_attempted_to_call_null_cb;
	else if ((transac->useCb))
		transac->cb(transac->data, transac->len);
	
	UART_BLOCKING_WRITE_CLEANUP:
		core_exitBlockingTransaction();
	
	return errorCode;
}

uart_error_t uart_blockingRead(uart_transaction_t *transac)
{
	uint16_t i;
	uart_error_t errorCode = uart_no_error;
	
	if (!gPeripheralInitialized)
		return uart_peripheral_uninitialized;
	
	core_enterBlockingTransaction();
	
	if ((transac->data == NULL) && (transac->len > 0))
	{
		errorCode = uart_attempted_to_dereference_null_ptr;
		goto UART_BLOCKING_READ_CLEANUP;
	}
	
	for (i = 0; i < transac->len; i++)
	{
		errorCode = uart_blockingReadByte(&transac->data[i]);
		if (errorCode != uart_no_error)
			goto UART_BLOCKING_READ_CLEANUP;
	}
	
	if ((transac->useCb) && (transac->cb == NULL))
		errorCode = uart_attempted_to_call_null_cb;
	else if ((transac->useCb))
		transac->cb(transac->data, transac->len);
	
	UART_BLOCKING_READ_CLEANUP:
		core_exitBlockingTransaction();
	
	return errorCode;
}

uart_error_t uart_getAsyncRxBytes(uart_transaction_t *transac, uint16_t maxNumBytesToRead)
{
  uart_error_t errorCode = gAsyncRxError;
  uint16_t i = 0;
	uint16_t tail;

  // ensure the peripheral is actually configured
  if (!gPeripheralInitialized)
		return uart_peripheral_uninitialized;
        
	// transac.len will notify the caller how many bytes are in the buffer
	transac->len = uart_numAsyncRxBytesInBuff();
	
	// check for a buffer overflow
  if (transac->len > gAsyncRxDataBuffLen)
  {
	  errorCode = uart_internal_buffer_overflow;
	  gAsyncRxDataBuffHead = gAsyncRxDataBuffTail = 0;
  }
  else
  {
	  for (i = 0; (i < transac->len) && (i < maxNumBytesToRead); i++)
	  {
		  tail = gAsyncRxDataBuffTail % gAsyncRxDataBuffLen;
		  transac->data[i] = gAsyncRxDataBuff[tail];
		  gAsyncRxDataBuffTail++;
	  }
  }

	if ((transac->useCb) && (transac->cb == NULL))
		errorCode = uart_attempted_to_call_null_cb;
	else if ((transac->useCb))
		transac->cb(transac->data, i);
	
  // reset async rx error
  gAsyncRxError = uart_no_error;
  return errorCode;
}

uart_error_t uart_setAsyncTxData(uart_transaction_t *transac)
{
	uart_error_t errorCode = gAsyncTxError;
	uint16_t i = 0;
	uint16_t txBuffSpace, head;
	
    if (!gPeripheralInitialized)
        return uart_peripheral_uninitialized;
    
	txBuffSpace = uart_availSpaceInAsyncTxBuff();
	
	// prevent a buffer overflow
	if (transac->len > txBuffSpace)
	{
		errorCode = uart_internal_buffer_overflow;
	}
	else if (transac->data == NULL)
	{
		errorCode = uart_attempted_to_dereference_null_ptr;
	}
	else
	{
		for (i = 0; i < transac->len; i++)
		{
			head = gAsyncTxDataBuffHead % gAsyncTxDataBuffLen;
			gAsyncTxDataBuff[head] = transac->data[i];
			gAsyncTxDataBuffHead++;
		}
		
		UART_ENABLE_DATA_REG_EMPTY_INT;
	}
	
	if ((transac->useCb) && (transac->cb == NULL))
		errorCode = uart_attempted_to_call_null_cb;
	else if ((transac->useCb))
		transac->cb(transac->data, i);
		
	// reset async tx error
	gAsyncTxError = uart_no_error;
	return errorCode;
}

uint16_t uart_numAsyncRxBytesInBuff()
{
	if (gAsyncRxDataBuff == NULL)
		return 0;
	
	return (gAsyncRxDataBuffHead - gAsyncRxDataBuffTail);
}

uint16_t uart_availSpaceInAsyncTxBuff()
{
	if (gAsyncTxDataBuff == NULL)
		return 0;
	
	return (gAsyncTxDataBuffLen - (gAsyncTxDataBuffHead - gAsyncTxDataBuffTail));
}

void uart_flushAsyncRxBuff()
{
    gAsyncRxDataBuffTail = gAsyncRxDataBuffHead;
}

void uart_flushAsyncTxBuff()
{
    gAsyncTxDataBuffTail = gAsyncTxDataBuffHead;
}
   