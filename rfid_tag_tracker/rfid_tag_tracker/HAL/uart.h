/*
 * uart.h
 *
 * Created: 8/14/2017 2:30:34 PM
 *  Author: warre
 */ 
#ifndef UART_H_
#define UART_H_

#include "core.h"

#ifdef __cplusplus
extern "C" {
#endif

// special type definitions

typedef enum {
	no_parity = 0,
	even_parity = 1,
	odd_parity = 2
}uart_parity_t;

typedef enum {
	baud_2400 = 2400,
	baud_4800 = 4800,
	baud_9600 = 9600,
	baud_14400 = 14400,
	baud_19200 = 19200,
	baud_28800 = 28800,
	baud_38400 = 38400,
	baud_57600 = 57600,
	baud_76800 = 76800,
	baud_115200 = 115200,
	baud_230400 = 230400,
	baud_250000 = 250000,
	baud_500000 = 500000,
	baud_1000000 = 1000000	
}uart_baud_rates_t;

typedef struct {
	bool_t multiProcessorMode;
	uart_parity_t parity;
	uint8_t numDataBits;
	uint8_t numStopBits;
	uart_baud_rates_t baudRate;
}uart_params_t;

typedef struct {
	uint8_t *data;
	uint16_t len;
	bool_t useCb;
	void (*cb) ();	
}uart_transaction_t;

// initializer
#define UART_INIT_TRANSAC_OBJ(x) {\
	x.data = NULL;\
	x.len = 0;\
	x.useCb = FALSE;\
	x.cb = NULL;\
}

// globally defined functions
uart_error_t uart_configurePeripheral(uart_params_t *params);
void uart_deConfigurePeripheral();
uart_error_t uart_configureAsyncRx(uint8_t *internalRxDataBuff, uint16_t internalRxDataBuffLen);
uart_error_t uart_configureAsyncTx(uint8_t *internalTxDataBuff, uint16_t internalTxDataBuffLen);
uart_error_t uart_blockingWrite(uart_transaction_t *transac);
uart_error_t uart_blockingRead(uart_transaction_t *transac);
uart_error_t uart_getAsyncRxBytes(uart_transaction_t *transac, uint16_t maxNumBytesToRead);
uart_error_t uart_setAsyncTxData(uart_transaction_t *transac);
uint16_t uart_numAsyncRxBytesInBuff();
uint16_t uart_availSpaceInAsyncTxBuff();
void uart_flushAsyncRxBuff();
void uart_flushAsyncTxBuff();

#ifdef __cplusplus
}
#endif /* __cplusplus */

#endif /* UART_H_ */