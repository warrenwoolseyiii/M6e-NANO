#include "UARTBus.h"

// public methods
UARTBus::UARTBus( uint8_t busNum )
{
	_busNum = busNum;
	_baudRate = 9600;
	_numDataBits = 8;
	_paritySettings = parity_none;
	_stopBits = 1;
}

void UARTBus::setBaudRate( uint32_t baudRate )
{
	_baudRate = baudRate;
}

void UARTBus::setParity( parity_settings_t parity )
{
	_paritySettings = parity;
}

void UARTBus::setNumStopBits( uint8_t stopBits )
{
	_stopBits = stopBits;
}

void UARTBus::setNumDataBits( uint8_t dataBits )
{
	_numDataBits = dataBits;
}

bool UARTBus::startBus()
{
	uart_params_t params;
	this->parameterizeSettings( params );

	if (uart_configurePeripheral( &params ) != uart_no_error)
		return false;
	if (uart_configureAsyncRx( _rxBuf, UART_BUFFER_LEN ) != uart_no_error)
		return false;
	if (uart_configureAsyncTx( _txBuf, UART_BUFFER_LEN ) != uart_no_error)
		return false;

	return true;
}

uint16_t UARTBus::asyncWriteBuf( uint8_t *buf, uint16_t numBytesToWrite )
{
	configureTransacObj( _txTransac, buf, numBytesToWrite );
	if (uart_setAsyncTxData( &_txTransac ) != uart_no_error)
		return 0;

	return numBytesToWrite;
}

uint16_t UARTBus::blockingWriteBuf( uint8_t *buf, uint16_t numBytesToWrite, uint32_t timeOutInMillis )
{
	bool retry = true;
	configureTransacObj( _txTransac, buf, numBytesToWrite );
	do 
	{
		if (retry)
		{	
			if (uart_setAsyncTxData( &_txTransac ) == uart_no_error)
				retry = false;
		}
		else
		{
			if (uart_availSpaceInAsyncTxBuff() == UART_BUFFER_LEN)
				return numBytesToWrite;
		}

		DELAY_MS_CONST(1);
	} while (timeOutInMillis--);
	
	this->flushTx();
	return 0;
}

uint16_t UARTBus::numAsyncRxBytesAvailable()
{
	return uart_numAsyncRxBytesInBuff();
}

uint16_t UARTBus::getAsyncRxBytes( uint8_t *buf, uint16_t numBytesToGet )
{
	uint16_t numBytesAvail = this->numAsyncRxBytesAvailable();
	if (numBytesAvail < numBytesToGet)
		numBytesToGet = numBytesAvail;

	this->configureTransacObj( _rxTransac, buf, numBytesToGet );
	if (uart_getAsyncRxBytes( &_rxTransac, numBytesToGet ) != uart_no_error)
		return 0;

	return numBytesToGet;
}

uint16_t UARTBus::blockingReadBuf( uint8_t *buf, uint16_t numBytesToRead, uint32_t timeOutInMillis )
{
	configureTransacObj( _rxTransac, buf, numBytesToRead );
	do
	{
		if (this->numAsyncRxBytesAvailable() >= numBytesToRead)
			if (uart_getAsyncRxBytes( &_rxTransac, numBytesToRead ) == uart_no_error)
				return numBytesToRead;

		DELAY_MS_CONST(1);
	} while (timeOutInMillis--);
	
	this->flushRx();
	return 0;
}

void UARTBus::flushTx()
{
	uart_flushAsyncTxBuff();
}

void UARTBus::flushRx()
{
	uart_flushAsyncRxBuff();
}

// private methods
void UARTBus::translateBaudToEnum( uart_baud_rates_t &baud )
{
	switch (_baudRate)
	{
		case 2400:
			baud = baud_2400;
		break;
		case 4800:
			baud = baud_4800;
		break;
		case 9600:
			baud = baud_9600;
		break;
		case 14400:
			baud = baud_14400;
		break;
		case 19200:
			baud = baud_19200;
		break;
		case 28800:
			baud = baud_28800;
		break;
		case 38400:
			baud = baud_38400;
		break;
		case 57600:
			baud = baud_57600;
		break;
		case 76800:
			baud = baud_76800;
		break;
		case 115200:
			baud = baud_115200;
		break;
		case 230400:
			baud = baud_230400;
		break;
		case 250000:
			baud = baud_250000;
		break;
		case 500000:
			baud = baud_500000;
		break;
		case 1000000:
			baud = baud_1000000;
		break;
		default:
			baud = baud_9600;
		break;
	}
}

void UARTBus::parameterizeSettings( uart_params_t &params )
{
	// TODO: if there are multiple buses we need to parameterize that as well
	params.multiProcessorMode = FALSE;
	params.numDataBits = _numDataBits;
	params.numStopBits = _stopBits;
	params.parity = (uart_parity_t)_paritySettings;	// in the HAL the enumerations are 1 to 1 cause I wrote them :)
	this->translateBaudToEnum( params.baudRate );
}

void UARTBus::configureTransacObj( uart_transaction_t &transac, uint8_t *buf, uint16_t len )
{
	UART_INIT_TRANSAC_OBJ( transac );
	transac.data = buf;
	transac.len = len;
}