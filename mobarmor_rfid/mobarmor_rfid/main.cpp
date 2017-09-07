/*
 * mobarmor_rfid.cpp
 *
 * Created: 9/6/2017 10:35:19 PM
 * Author : warre
 */ 

#include <avr/io.h>

#include <hal/atmega328p/core/core.h>
#include <hal/atmega328p/uart/uart.h>

#include <SparkFun_UHF_RFID_Reader.h>

int main(void)
{
	RFID nano;
	uart_params_t params;

	params.baudRate = baud_115200;
	params.multiProcessorMode = FALSE;
	params.numDataBits = 8;
	params.numStopBits = 1;
	params.parity = no_parity;

	while (!nano.begin(params));

	while (uart_numAsyncRxBytesInBuff() > 0)
		uart_flushAsyncRxBuff();

	nano.getVersion();
	if (nano.msg[0] != ALL_GOOD)
		return 0;

	/* Replace with your application code */
	while (1) 
	{
	}
}

