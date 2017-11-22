/*
 * rfid_tag_tracker.cpp
 *
 * Created: 10/30/2017 8:47:22 PM
 * Author : warre
 */ 

#include <avr/io.h>
#include "m6e_nano/m6eNano.h"
#include "util.h"

RFID gTagReader;

bool initTagReader()
{
	if (!gTagReader.start())
		return false;

	gTagReader.getReadPower();
}

int main()
{
	DELAY_MS_CONST( 2000 );
	core_enableGlobalInt();
	initMillis();
	initTagReader();

	while( true )
	{
		
	}
}

