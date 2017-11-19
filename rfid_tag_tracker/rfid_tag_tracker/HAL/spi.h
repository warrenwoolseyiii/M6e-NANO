/*
 * spi.h
 *
 * Created: 7/27/2017 4:44:53 PM
 *  Author: warre
 */ 
#ifndef SPI_H_
#define SPI_H_

#include "core.h"
#include "gpio.h"

#ifdef __cplusplus
extern "C" {
#endif

// constants
#define SCK0_PORT port_b
#define SCK0_PIN pin_5
#define MOSI0_PORT port_b
#define MOSI0_PIN pin_3
#define MISO0_PORT port_b
#define MISO0_PIN pin_4

// special type definitions
typedef enum {
	spi_master_mode,
	spi_slave_mode
}spi_modes_t;

typedef enum {
	polarity_high,
	polarity_low	
}spi_polarity_t;

typedef enum {
	lead_sample,
	lead_setup
}spi_phase_t;

typedef enum {
	msb_first,
	lsb_first	
}spi_data_order_t;

typedef enum {
	spi_bus0,
	spi_bus1
}spi_bus_t;

typedef struct {
	uint32_t maxBitRate;
	spi_data_order_t dataOrder;
	spi_polarity_t clockPolarity;
	spi_phase_t clockPhase;
	spi_bus_t bus;
	bool_t csActiveHigh;
	gpio_t cs;
}spi_device_config_t;

typedef struct {
	spi_device_config_t *deviceCfg;
	uint8_t *data;
	uint16_t len;
	bool_t useCb;
	void (*cb) ();
}spi_transaction_t;

// initializer
#define SPI_INIT_TRANSAC_OBJ(x) {\
	x.data = NULL;\
	x.len = 0;\
	x.useCb = FALSE;\
	x.cb = NULL;\
}

// exposed function prototypes
uint32_t spi_configureAsMaster(uint32_t bitRate, spi_bus_t bus);
void spi_configureAsSlave(spi_bus_t bus);
void spi_deConfigure(spi_bus_t bus);
spi_error_t spi_blockingReadWrite(spi_transaction_t *transac);

#ifdef __cplusplus
}
#endif /* __cplusplus */

#endif /* SPI_H_ */