/*
 * twi.c
 *
 * Created: 7/17/2017 6:28:19 PM
 *  Author: warre
 */ 
#include "twi.h"

// constants
#define TWI_MAX_BITRATE 400000UL

// macro functions
#define TWI_IS_ENABLED ((TWCR & (1 << TWEN)) ? TRUE : FALSE)
#define TWI_ENABLE (TWCR |= (1 << TWEN))
#define TWI_DISABLE (TWCR &= ~(1 << TWEN))

#define TWI_IS_INT_ENABLED ((TWCR & (1 << TWIE)) ? TRUE : FALSE)
#define TWI_ENABLE_INT (TWCR |= (1 << TWIE))
#define TWI_DISABLE_INT (TWCR &= ~(1 << TWIE))

#define TWI_IS_INT_FLAG_SET ((TWCR & (1 << TWINT)) ? TRUE : FALSE)
#define TWI_CLR_INT_FLAG (TWCR |= (1 << TWINT))

#define TWI_SET_START (TWCR |= (1 << TWSTA))
#define TWI_CLR_START (TWCR &= ~(1 << TWSTA))
#define TWI_GET_START (((TWCR & (1 << TWSTA)) > 0) ? TRUE : FALSE)

#define TWI_STOP (TWCR |= (1 << TWSTO))

#define TWI_GET_STATUS (TWSR & 0xf8)

#define TWI_SET_SLAVE_ADDR(x) (TWAR = x)

#define TWI_SET_ACK (TWCR |= (1 << TWEA))
#define TWI_SET_NACK (TWCR &= ~(1 << TWEA))

#define TWI_SET_DATA_REG(x) (TWDR = x)
#define TWI_GET_DATA_REG(x) (x = TWDR)

#define TWI_RST_PERIPHERAL {\
TWCR = 0;\
TWBR = 0;\
TWSR = 0;\
TWAR = 0xfe;\
TWAMR = 0;\
}

// file specific global variables
static twi_modes_t gTwiMode;
static bool_t gIsInitialized = FALSE;
static uint16_t gTimeOutUs = 5000;	// default to 5000 us timeouts (which is a 1600 baud bitrate)

// locally defined functions
uint32_t twi_setBitRateParams(uint32_t bitRate)
{
	// calculates the bitrate divider and prescaler based on the register settings of the TWI peripheral
	// see Atmel-42735-8-bit-AVR-Microcontroller-ATmega328-328P_Datasheet.pdf for more details.
	uint8_t preScaler = 1;
	uint32_t bitRateDivider = 0;
	
	if (bitRate > TWI_MAX_BITRATE || bitRate == 0)
		bitRate = TWI_MAX_BITRATE;
		
	
	// if the quotient is less than 0 than we are requesting a bitrate that is out of possible range.
	// therefore just run the peripheral as fast as possible
	if (((F_CPU / bitRate) - 16) >= 0)
	{
		// bitRate = ((F_CPU) / (16 + (2 * bitRateDivider * preScaler)))
		for (; preScaler <= 64; preScaler <<= 2)
		{
			bitRateDivider = (uint32_t)((float_t)((F_CPU / bitRate) - 16.0) * (float_t)(1.0 / (2.0 * preScaler)));
		
			if (bitRateDivider <= 255)
				break;
			else
				bitRateDivider = 1;
		}
	}
	
	TWBR = (uint8_t)(bitRateDivider & 0xff);
	TWSR &= ~(0b11);
	
	switch (preScaler)
	{
		case 4:
			TWSR |= (0b01);
			break;
		case 16:
			TWSR |= (0b10);
			break;
		case 64:
			TWSR |= (0b11);
			break;
	}
	
	// calculate that actual bit rate and timeout. timeout is calculated to be 4 times the amount 
	// of time it should take to transmit a single byte
	bitRate = ((F_CPU) / (16 + (2 * bitRateDivider * preScaler)));
	gTimeOutUs = (uint16_t)((float_t)(36000000.0F / bitRate));
	
	return bitRate;
}

twi_error_t twi_pollIntFlag()
{
	uint16_t cntrUs;
	
	for (cntrUs = 0; cntrUs <= gTimeOutUs; cntrUs += 10)
	{
		DELAY_US_CONST(10);
		
		if (TWI_IS_INT_FLAG_SET)
			return twi_no_error;
	}
	
	return twi_timeout;
}

twi_error_t twi_checkStatus()
{
	uint8_t status = TWI_GET_STATUS;
	
	if (gTwiMode == twi_master_mode_tx || gTwiMode == twi_master_mode_rx)
	{
		switch (status)
		{
			case 0x08:
				// start transmission sent
				return twi_no_error;
			case 0x10:
				// repeat start transmission sent
				return twi_no_error;
			case 0x18:
				// sla+w was sent & got ack
				return twi_no_error;
			case 0x20:
				// sla+w was sent & got nack
				return twi_received_nack;
			case 0x28:
				// data byte was sent & got ack
				return twi_no_error;
			case 0x30:
				// data byte was sent & got nack
				return twi_received_nack;
			case 0x38:
				// arbitration lost
				return twi_arbitration_lost;
			case 0x40:
				// sla+r was sent & got ack
				return twi_no_error;
			case 0x48:
				// sla+r was sent & got nack
				return twi_received_nack;
			case 0x50:
				// data byte was received & sent ack
				return twi_no_error;
			case 0x58:
				// data byte was received & sent nack
				return twi_sent_nack;
			default:
				// unknown code
				return twi_unknown_status_code;
		}
	}
	
	return twi_no_error;
}

void twi_resolveError(twi_error_t error)
{
	if (gTwiMode == twi_master_mode_tx || gTwiMode == twi_master_mode_rx)
	{
		switch (error)
		{
			case twi_no_error:
				break;
			case twi_peripheral_uninitialized:
				break;
			case twi_attempted_to_dereference_null_ptr:
				break;
			case twi_timeout:
				// a timeout will usually mean the addressed slave is hanging or the peripheral itself is hanging
				// so we need to figure out a clean way to reset the peripheral and the device
				break;
			case twi_received_nack:
				if (TWI_GET_START)
					TWI_CLR_START;
				
				TWI_STOP;
				TWI_CLR_INT_FLAG;
				break;
			case twi_sent_nack:
				// this is not an error because it is set on purpose in user code so simply return the status and
				// the caller will handle it properly
				break;
			case twi_arbitration_lost:
				if (TWI_GET_START)
					TWI_CLR_START;
					
				TWI_CLR_INT_FLAG;
				break;
			case twi_unknown_status_code:
				break;
		}
	}
}

twi_error_t twi_blockTillStepCompletes()
{
	twi_error_t rtnCode;
	
	rtnCode = twi_pollIntFlag();
	if (rtnCode != twi_no_error)
	{
		twi_resolveError(rtnCode);
		return rtnCode;
	}
	
	rtnCode = twi_checkStatus();
	if (rtnCode != twi_no_error)
	{
		twi_resolveError(rtnCode);
		return rtnCode;
	}
	
	return rtnCode;
}

twi_error_t twi_blockingStartMasterTransaction(uint8_t slaveAddrRW)
{
	twi_error_t rtnCode;
	
	if (!TWI_IS_ENABLED)
		TWI_ENABLE;
	
	// start sequence
	TWI_SET_START;
	TWI_CLR_INT_FLAG;
	if ((rtnCode = twi_blockTillStepCompletes()) != twi_no_error)
		return rtnCode;
		
	// send the slave addr + rw
	TWI_SET_DATA_REG(slaveAddrRW);
	TWI_CLR_START;
	TWI_CLR_INT_FLAG;
	
	return twi_blockTillStepCompletes();
}

// globally defined functions
uint32_t twi_configurePeripheralAsMaster(uint32_t bitRate, bool_t enable)
{
	TWI_DISABLE_INT;
	TWI_DISABLE;
	
	bitRate = twi_setBitRateParams(bitRate);
	
	if (enable)
		TWI_ENABLE;
		
	gIsInitialized = TRUE;
	
	return bitRate;
}

void twi_configurePeripheralAsSlave(uint8_t slaveAddress, bool_t enable)
{
	TWI_DISABLE_INT;
	TWI_DISABLE;
	
	TWI_SET_SLAVE_ADDR(slaveAddress);
	
	if (enable)
		TWI_ENABLE;
		
	gIsInitialized = TRUE;
}

void twi_deConfigurePeripheral()
{
	gIsInitialized = FALSE;
	gTimeOutUs = 5000;
	TWI_RST_PERIPHERAL;
}

void twi_enableInterrupts(bool_t enable)
{
	if (enable)
		TWI_ENABLE_INT;
	else
		TWI_DISABLE_INT;
}

twi_error_t twi_blockingWrite(twi_transaction_t *transac, bool_t endWithStopCondition)
{
	uint16_t i;
	twi_error_t rtnCode = twi_no_error;
	
	core_enterBlockingTransaction();
	
	if (!gIsInitialized)
	{
		rtnCode = twi_peripheral_uninitialized;
		goto TWI_BLOCKING_WRITE_CLEANUP;
	}
	
	if ((transac->len > 0) && (transac->data == NULL))
	{
		rtnCode = twi_attempted_to_dereference_null_ptr;
		goto TWI_BLOCKING_WRITE_CLEANUP;
	}
	
	gTwiMode = twi_master_mode_tx;
	
	// initiate transaction
	rtnCode = twi_blockingStartMasterTransaction((transac->slaveAddr << 1) | transac->rw);
	if (rtnCode != twi_no_error)
		goto TWI_BLOCKING_WRITE_CLEANUP;
	
	// send the data
	for (i = 0; i < transac->len; i++)
	{
		TWI_SET_DATA_REG(transac->data[i]);
		TWI_CLR_INT_FLAG;
		rtnCode = twi_blockTillStepCompletes();
		if (rtnCode != twi_no_error)
			goto TWI_BLOCKING_WRITE_CLEANUP;
	}
	
	if (endWithStopCondition)
	{	
		TWI_STOP;
		TWI_CLR_INT_FLAG;
	}
	
	if ((transac->useCb) && (transac->cb != NULL))
		transac->cb(transac->data, transac->len);
	else if ((transac->useCb) && (transac->cb == NULL))
		rtnCode = twi_attempted_to_dereference_null_ptr;
	
	TWI_BLOCKING_WRITE_CLEANUP:
		core_exitBlockingTransaction();
	
	return rtnCode;
}

twi_error_t twi_blockingRead(twi_transaction_t *transac)
{
	uint16_t i;
	twi_error_t rtnCode = twi_no_error;
	
	core_enterBlockingTransaction();
	
	if (!gIsInitialized)
	{
		rtnCode = twi_peripheral_uninitialized;
		goto TWI_BLOCKING_READ_CLEANUP;
	}
		
	if ((transac->len > 0) && (transac->data == NULL))
	{
		rtnCode = twi_attempted_to_dereference_null_ptr;
		goto TWI_BLOCKING_READ_CLEANUP;
	}
	
	gTwiMode = twi_master_mode_rx;
	
	// initiate transaction
	rtnCode = twi_blockingStartMasterTransaction((transac->slaveAddr << 1) | transac->rw);
	if (rtnCode != twi_no_error)
		goto TWI_BLOCKING_READ_CLEANUP;
		
	// read the data
	for (i = 0; i < transac->len; i++)
	{
		if (i == (transac->len - 1))
			TWI_SET_NACK;
		else
			TWI_SET_ACK;
		
		TWI_CLR_INT_FLAG;
		rtnCode = twi_blockTillStepCompletes();
		if (rtnCode == twi_no_error)
		{
			TWI_GET_DATA_REG(transac->data[i]);
			TWI_CLR_INT_FLAG;
		}
		else if (rtnCode == twi_sent_nack)
		{
			TWI_GET_DATA_REG(transac->data[i]);
			TWI_STOP;
			TWI_CLR_INT_FLAG;
			rtnCode = twi_no_error;
			break;
		}
		else
		{
			TWI_STOP;
			TWI_CLR_INT_FLAG;
			goto TWI_BLOCKING_READ_CLEANUP;
		}
	}
	
	if ((transac->useCb) && (transac->cb != NULL))
		transac->cb(transac->data, transac->len);
	else if ((transac->useCb) && (transac->cb == NULL))
		rtnCode = twi_attempted_to_dereference_null_ptr;
	
	TWI_BLOCKING_READ_CLEANUP:
		core_exitBlockingTransaction();
		
	return rtnCode;
}
