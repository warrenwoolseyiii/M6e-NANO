/*
 * mobarmor_rfid.cpp
 *
 * Created: 9/6/2017 10:35:19 PM
 * Author : warre
 */ 

#include <avr/io.h>

#include <hal/atmega328p/core/core.h>
#include <hal/atmega328p/uart/uart.h>
#include <hal/atmega328p/gpio/gpio.h>

#include <SparkFun_UHF_RFID_Reader.h>

bool_t setupNano(long baudRate);

RFID nano;

int main(void)
{
	gpio_t LED;

	LED.port = port_b;
	LED.pin = pin_5;
	gpio_setMode(LED, output);
	
	/* Replace with your application code */
	while (1) 
	{
		if (!setupNano(38400))
			gpio_outputLogicHigh(LED);
		else
			gpio_outputLogicLow(LED);
		DELAY_MS_CONST(500);
	}
}


//Gracefully handles a reader that is already configured and already reading continuously

//Because Stream does not have a .begin() we have to do this outside the library

bool_t setupNano(long baudRate)
{
	uart_params_t params;

	params.baudRate = baud_38400;
	params.multiProcessorMode = FALSE;
	params.numDataBits = 8;
	params.numStopBits = 1;
	params.parity = no_parity;

	if (!nano.begin(params))
		return FALSE;

	while (uart_numAsyncRxBytesInBuff() > 0)
		uart_flushAsyncRxBuff();

	nano.getVersion();
	if (nano.msg[0] == ERROR_WRONG_OPCODE_RESPONSE)
	{
		nano.stopReading();
		DELAY_MS_CONST(1500);
	}
	else
	{
		params.baudRate = baud_115200;
		params.multiProcessorMode = FALSE;
		params.numDataBits = 8;
		params.numStopBits = 1;
		params.parity = no_parity;
		
		nano.begin(params);
		nano.setBaud(baudRate); //Tell the module to go to the chosen baud rate. Ignore the response msg

		params.baudRate = baud_38400;
		nano.begin(params);
	}

	//Test the connection
	nano.getVersion();
	if (nano.msg[0] != ALL_GOOD) 
		return (false); //Something is not right

	//The M6E has these settings no matter what
	nano.setTagProtocol(); //Set protocol to GEN2
	nano.setAntennaPort(); //Set TX/RX antenna ports to 1

	return (true); //We are ready to rock
}