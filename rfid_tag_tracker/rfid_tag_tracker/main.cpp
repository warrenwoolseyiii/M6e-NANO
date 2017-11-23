/*
 * rfid_tag_tracker.cpp
 *
 * Created: 10/30/2017 8:47:22 PM
 * Author : warre
 */ 

#include <avr/io.h>
#include "m6e_nano/m6eNano.h"
#include "util.h"
#include "message/message.h"

RFID gTagReader;

void testMessage()
{
	MSG msg1, msg2, msg3;
	uint8_t randomData[64];
	uint8_t *randomPayload;

	memset( (void *)randomData, 1, 64 );
	msg1.setParams( 1, 16, randomData );
	memset( (void *)randomData, 2, 64 );
	msg2.setParams( 2, 32, randomData );
	memset( (void *)randomData, 1, 64 );
	msg3.setParams( 3, 64, randomData );

	if (msg3.getType() != 3)
		return;
	if (msg2.getSize() != 32)
		return;
	if (msg1.getPtrToPayload( randomPayload ) != msg1.getSize())
		return;
}

bool initTagReader()
{
	if (!gTagReader.start())
		return false;

	gTagReader.setReadPower( 500 );
	gTagReader.getReadPower();
}

int main()
{
	DELAY_MS_CONST( 2000 );
	core_enableGlobalInt();
	initMillis();
	initTagReader();

	testMessage();

	while( true )
	{
		
	}
}

