/*
 * spi.c
 *
 * Created: 7/27/2017 4:45:08 PM
 *  Author: warre
 */ 
#include "spi.h"

// constants
#define SPI_TIMEOUT_MICROS 5000UL

// macro functions
#define SPI0_IS_ENABLED ((SPCR & (1 << SPE)) ? TRUE : FALSE)
#define SPI0_ENABLE (SPCR |= (1 << SPE))
#define SPI0_DISABLE (SPCR &= ~(1 << SPE))

#define SPI0_SET_AS_MASTER (SPCR |= (1 << MSTR))
#define SPI0_SET_AS_SLAVE (SPCR &= ~(1 << MSTR))

#define SPI0_IS_INT_ENABLED ((SPCR & (1 << SPIE)) ? TRUE : FALSE)
#define SPI0_ENABLE_INT (SPCR |= (1 << SPIE))
#define SPI0_DISABLE_INT (SPCR &= ~(1 << SPIE))

#define SPI0_IS_INT_FLAG_SET ((SPSR & (1 << SPIF)) ? TRUE : FALSE)

#define SPI0_CHECK_WRT_COLLISION_FLAG ((SPSR & (1 << WCOL)) ? TRUE : FALSE)

#define SPI0_SET_MSB_FIRST (SPCR &= ~(1 << DORD))
#define SPI0_SET_LSB_FIRST (SPCR |= (1 << DORD))

#define SPI0_SET_CLK_POL_IDLE_HIGH (SPCR |= (1 << CPOL))
#define SPI0_SET_CLK_POL_IDLE_LOW (SPCR &= ~(1 << CPOL))

#define SPI0_SET_CLK_PHASE_LEAD_SAMPLE (SPCR &= ~(1 << CPHA))
#define SPI0_SET_CLK_PHASE_LEAD_SETUP (SPCR |= (1 << CPHA))

#define SPI0_IS_DBL_SPEED_ENABLED ((SPSR & (1 << SPI2X)) ? TRUE : FALSE)
#define SPI0_ENABLE_DBL_SPEED (SPSR |= (1 << SPI2X))
#define SPI0_DISABLE_DBL_SPEED (SPSR &= ~(1 << SPI2X))

#define SPI0_WRITE_DATA_REG(x) (SPDR = x)
#define SPI0_READ_DATA_REG(x) (x = SPDR)

#define SPI0_RESET_BITRATE_DIVIDER (SPCR &= ~(0b11))

#define SPI0_RST_PERIPHERAL {\
SPCR = 0;\
SPSR = 0;\
}

// file specific global variables
static spi_modes_t gSPIModeBus0 = spi_slave_mode;
static uint32_t gCurrentBitRateBus0 = 0;

// locally defined functions
uint32_t spi_setBitRate(uint32_t maxBitRate, spi_bus_t bus)
{
	// spi_setBitRate sets the bit rate so that it is under or equal to the maximum allowable bit rate
	// passed via the parameter maxBitRate
	uint8_t divider;
	uint32_t bitRate;
	
	for (divider = 1; divider < 8; divider++)
	{
		bitRate = F_CPU >> divider;
		
		if (bitRate <= maxBitRate)
			break;
	}	
	
	if (bus == spi_bus0)
	{
		switch (divider)
		{
			case 1:
				// fosc/2
				SPI0_ENABLE_DBL_SPEED;
				SPI0_RESET_BITRATE_DIVIDER;
				break;
			case 2:
				// fosc/4
				SPI0_DISABLE_DBL_SPEED;
				SPI0_RESET_BITRATE_DIVIDER;
				break;
			case 3:
				// fosc/8
				SPI0_ENABLE_DBL_SPEED;
				SPI0_RESET_BITRATE_DIVIDER;
				SPCR |= 0b1;
				break;
			case 4:
				// fosc/16
				SPI0_DISABLE_DBL_SPEED;
				SPI0_RESET_BITRATE_DIVIDER;
				SPCR |= 0b1;
				break;
			case 5:
				// fosc/32
				SPI0_ENABLE_DBL_SPEED;
				SPI0_RESET_BITRATE_DIVIDER;
				SPCR |= 0b10;
				break;
			case 6:
				// fosc/64
				SPI0_DISABLE_DBL_SPEED;
				SPI0_RESET_BITRATE_DIVIDER;
				SPCR |= 0b10;
				break;
			case 7:
				// fosc/128
				SPI0_DISABLE_DBL_SPEED;
				SPI0_RESET_BITRATE_DIVIDER;
				SPCR |= 0b11;
				break;
			default:
				// fosc/128
				bitRate = F_CPU >> 7;
				SPI0_DISABLE_DBL_SPEED;
				SPI0_RESET_BITRATE_DIVIDER;
				SPCR |= 0b11;
				break;
		}
	
		gCurrentBitRateBus0 = bitRate;
	}
	
	return bitRate;
}

void spi_activateSlave(gpio_t cs, bool_t activeHigh)
{
	if (activeHigh)
		gpio_outputLogicHigh(&cs);
	else
		gpio_outputLogicLow(&cs);
}

void spi_deactivateSlave(gpio_t cs, bool_t activeHigh)
{
	if (activeHigh)
		gpio_outputLogicLow(&cs);
	else
		gpio_outputLogicHigh(&cs);
}

void spi_configurePeripheralForDevice(spi_device_config_t *config)
{
	if (config->bus == spi_bus0)
	{
		if (gSPIModeBus0 != spi_master_mode)
			spi_configureAsMaster(config->maxBitRate, config->bus);
	
		if (config->dataOrder == msb_first)
			SPI0_SET_MSB_FIRST;
		else
			SPI0_SET_LSB_FIRST;
		
		if (config->clockPolarity == polarity_high)
			SPI0_SET_CLK_POL_IDLE_HIGH;
		else
			SPI0_SET_CLK_POL_IDLE_LOW;
			
		if (config->clockPhase == lead_sample)
			SPI0_SET_CLK_PHASE_LEAD_SAMPLE;
		else
			SPI0_SET_CLK_PHASE_LEAD_SETUP;
			
		spi_setBitRate(config->maxBitRate, config->bus);
	}
}

spi_error_t spi_pollIntFlag(spi_bus_t bus)
{
	uint16_t i;
	
	for (i = 0; i < SPI_TIMEOUT_MICROS; i+=1)
	{
		switch (bus)
		{
			case spi_bus0:
				if (SPI0_IS_INT_FLAG_SET)
					return spi_no_error;
				break;
			case spi_bus1:
				// Not implemented
				break;
		}
		
		DELAY_US_CONST(1);
	}
	
	return spi_timeout;
}

// globally defined functions
uint32_t spi_configureAsMaster(uint32_t bitRate, spi_bus_t bus)
{
	gpio_t sck, miso, mosi;
	
	if (bus == spi_bus0)
	{
		SPI0_DISABLE;
		SPI0_DISABLE_INT;
		
		sck.port = SCK0_PORT;
		sck.pin = SCK0_PIN;
		miso.port = MISO0_PORT;
		miso.pin = MISO0_PIN;
		mosi.port = MOSI0_PORT;
		mosi.pin = MOSI0_PIN;
		
		gpio_setMode(&sck, output);
		gpio_setMode(&mosi, output);
		gpio_setMode(&miso, input);
		
		bitRate = spi_setBitRate(bitRate, bus);
		gSPIModeBus0 = spi_master_mode;
		
		SPI0_SET_AS_MASTER;
		SPI0_ENABLE;
	}
	
	return bitRate;
}

void spi_configureAsSlave(spi_bus_t bus)
{
	gpio_t sck, miso, mosi;
	
	if (bus == spi_bus0)
	{	
		SPI0_DISABLE;
		SPI0_DISABLE_INT;
		
		sck.port = SCK0_PORT;
		sck.pin = SCK0_PIN;
		miso.port = MISO0_PORT;
		miso.pin = MISO0_PIN;
		mosi.port = MOSI0_PORT;
		mosi.pin = MOSI0_PIN;
		
		gpio_setMode(&sck, input);
		gpio_setMode(&mosi, output);
		gpio_setMode(&miso, input);
		
		gSPIModeBus0 = spi_slave_mode;
		
		SPI0_SET_AS_SLAVE;
		SPI0_ENABLE;
	}
}

void spi_deConfigure(spi_bus_t bus)
{
	if (bus == spi_bus0)
		SPI0_RST_PERIPHERAL;
}

spi_error_t spi_blockingReadWrite(spi_transaction_t *transac)
{
	spi_error_t errorCode = spi_no_error;
	uint16_t i;
	
	if ((transac->len > 0) && (transac->data == NULL))
	{
		errorCode = spi_attempted_to_dereference_null_ptr;
		return errorCode;
	}
		
	core_enterBlockingTransaction();
	spi_configurePeripheralForDevice(transac->deviceCfg);
	spi_activateSlave(transac->deviceCfg->cs, transac->deviceCfg->csActiveHigh);

	for (i = 0; i < transac->len; i++)
	{
		if (transac->deviceCfg->bus == spi_bus0)
		{
			SPI0_WRITE_DATA_REG(transac->data[i]);
			
			errorCode = spi_pollIntFlag(spi_bus0);
			if (errorCode != spi_no_error)
				break;
			
			SPI0_READ_DATA_REG(transac->data[i]);
		}
	}
	
	spi_deactivateSlave(transac->deviceCfg->cs, transac->deviceCfg->csActiveHigh);
	
	if (transac->useCb && (transac->cb != NULL) && (errorCode == spi_no_error))
		transac->cb(transac->data, transac->len);
	else if (transac->useCb && (transac->cb == NULL) && (errorCode == spi_no_error))
		errorCode = spi_attempted_to_dereference_null_ptr;
	
	core_exitBlockingTransaction();
	
	return errorCode;
}