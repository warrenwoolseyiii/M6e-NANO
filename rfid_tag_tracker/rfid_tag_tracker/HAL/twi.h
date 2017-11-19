/*
 * twi.h
 *
 * Created: 7/17/2017 6:28:31 PM
 *  Author: warre
 */ 
#ifndef TWI_H_
#define TWI_H_

#include "core.h"

#ifdef __cplusplus
extern "C" {
#endif

// special type definitions
typedef enum {
	twi_master_mode_tx,
	twi_master_mode_rx,
	twi_slave_mode_tx,
	twi_slave_mode_rx
	}twi_modes_t;
	
typedef enum {
	read = 0x01,
	write = 0x00
	}twi_rw_t;
	
typedef struct {
	twi_rw_t rw;
	uint8_t slaveAddr;
	uint8_t *data;
	uint16_t len;
	bool_t useCb;
	void (*cb) ();
	}twi_transaction_t;

// initializer
#define TWI_INIT_TRANSAC_OBJ(x) {\
	x.rw = read;\
	x.slaveAddr = 0;\
	x.data = NULL;\
	x.len = 0;\
	x.useCb = FALSE;\
	x.cb = NULL;\
}

// exposed function prototypes
uint32_t twi_configurePeripheralAsMaster(uint32_t bitRate, bool_t enable);
void twi_configurePeripheralAsSlave(uint8_t slaveAddress, bool_t enable);
void twi_deConfigurePeripheral();
void twi_enableInterrupts(bool_t enable);
twi_error_t twi_blockingWrite(twi_transaction_t *transac, bool_t endWithStopCondition);
twi_error_t twi_blockingRead(twi_transaction_t *transac);

#ifdef __cplusplus
}
#endif /* __cplusplus */

#endif /* TWI_H_ */