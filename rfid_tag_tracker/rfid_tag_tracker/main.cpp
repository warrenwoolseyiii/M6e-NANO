/*
 * rfid_tag_tracker.cpp
 *
 * Created: 10/30/2017 8:47:22 PM
 * Author : warre
 */ 

#include <avr/io.h>
#include "m6e_nano/UARTBus.h"

UARTBus bus0 = UARTBus(0);

int main(void)
{	
	core_enableGlobalInt();
	bus0.setBaudRate( 115200 );
	bus0.setNumDataBits( 8 );
	bus0.setNumStopBits( 1 );
	bus0.setParity( parity_none );
	bus0.startBus();

	uint8_t dataFun[256];

	while (1)
	{
		uint16_t numRxBytes = bus0.numAsyncRxBytesAvailable();
		if (numRxBytes > 0)
		{
			bus0.getAsyncRxBytes(dataFun, numRxBytes);
			bus0.asyncWriteBuf(dataFun, numRxBytes);
		}
	}
}

