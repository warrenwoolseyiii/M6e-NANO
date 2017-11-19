/*
 * eeprom.h
 *
 * Created: 8/21/2017 3:53:21 PM
 *  Author: warre
 */ 
#ifndef EEPROM_H_
#define EEPROM_H_

#include "core.h"

#ifdef __cplusplus
extern "C" {
#endif

#define EEPROM_MAX_ADDR 1024

// special type definitions
typedef struct {
	uint16_t addr;
	uint8_t *data;
	uint16_t len;
}eeprom_transaction_t;

// initializer
#define EEPROM_INIT_TRANSC_OBJ(x) {\
x.addr = 0;\
x.data = NULL;\
x.len = 0;\
};

// globally defined functions
eeprom_error_t eeprom_blockingErase(eeprom_transaction_t *transac);
eeprom_error_t eeprom_blockingWrite(eeprom_transaction_t *transac);
eeprom_error_t eeprom_blockingRead(eeprom_transaction_t *transac);

#ifdef __cplusplus
}
#endif /* __cplusplus */

#endif /* EEPROM_H_ */