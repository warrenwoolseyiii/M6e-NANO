#ifndef UARTBUS_H_
#define UARTBUS_H_

// atmega328p specific UART header
#include "../HAL/uart.h"

#include <stdbool.h>

#define UART_BUFFER_LEN 128

typedef enum
{
	parity_none = 0,
	parity_odd = 1,
	parity_even = 2
}parity_settings_t;

class UARTBus
{
// variables
public:
protected:
private:
	uint8_t _txBuf[UART_BUFFER_LEN];
	uint8_t _rxBuf[UART_BUFFER_LEN];
	uint32_t _baudRate;
	parity_settings_t _paritySettings;
	uint8_t _stopBits, _busNum, _numDataBits;

	// private variables specific to the UART implementation
	uart_transaction_t _txTransac, _rxTransac;

// functions
public:
	UARTBus( uint8_t busNum = 0 );
	void setBaudRate( uint32_t baudRate );
	void setParity( parity_settings_t parity );
	void setNumStopBits( uint8_t stopBits );
	void setNumDataBits( uint8_t dataBits );
	bool startBus();
	uint16_t asyncWriteBuf( uint8_t *buf, uint16_t numBytesToWrite );
	uint16_t blockingWriteBuf( uint8_t *buf, uint16_t numBytesToWrite, uint32_t timeOutInMillis );
	uint16_t numAsyncRxBytesAvailable();
	uint16_t getAsyncRxBytes( uint8_t *buf, uint16_t numBytesToGet );
	uint16_t blockingReadBuf( uint8_t *buf, uint16_t numBytesToRead, uint32_t timeOutInMillis );
	void flushTx();
	void flushRx();
protected:
private:
	// methods specific to the hardware implementation of the processor's UART bus
	void translateBaudToEnum( uart_baud_rates_t &baud );
	void parameterizeSettings( uart_params_t &params );
	void configureTransacObj( uart_transaction_t &transac, uint8_t *buf, uint16_t len );
};

#endif /* UARTBUS_H_ */
