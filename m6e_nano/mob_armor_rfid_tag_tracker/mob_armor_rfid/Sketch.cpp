#include <Arduino.h>
#include <Wire.h>
#include <SPI.h>
#include <EEPROM.h>
#include <SoftwareSerial.h>
#include <SparkFun_UHF_RFID_Reader.h>

#define LED 13

// globally defined objects
RFID gNano;
SoftwareSerial gSoftSerial(2, 3);
boolean gSystemReady;
uint32_t gNumTags = 0;

// file specific functions
boolean setupNano(long baudRate);
void scanForTags();

void setup() 
{
	gSystemReady = true;

	if (!setupNano(38400))
	{
		// log setup failure here
		gSystemReady = false;
	}

	pinMode(LED, OUTPUT);
	digitalWrite(LED, LOW);
}

void loop() 
{
	TRY_AGAIN:
	if (!gSystemReady)
	{
		setup();
		goto TRY_AGAIN;
	}

	scanForTags();
}

boolean setupNano(long baudRate)
{
	// start the nano, assume the module is running at the correct baud rate
	gNano.begin(gSoftSerial);
	gSoftSerial.begin(baudRate);
	while(!gSoftSerial);

	// About 200ms from power on the module will send its firmware version at 115200. We need to ignore this.
	while(gSoftSerial.available()) 
		gSoftSerial.read();
	
	gNano.getVersion();
	if (gNano.msg[0] == ERROR_WRONG_OPCODE_RESPONSE)
	{
		// This happens if the baud rate is correct but the module is doing a continuous read
		gNano.stopReading();
		Serial.println(F("Module continuously reading. Asking it to stop..."));
		delay(1500);
	}
	else
	{
		// The module did not respond so assume it's just been powered on and communicating at 115200bps
		// which is the default baud rate.
		gSoftSerial.begin(115200);
		gNano.setBaud(baudRate);
		gSoftSerial.begin(baudRate);
	}

	// Test the connection
	gNano.getVersion();
	if (gNano.msg[0] != ALL_GOOD) 
		return (false);

	// The M6E has these settings no matter what
	gNano.setTagProtocol(); // Set protocol to GEN2
	gNano.setAntennaPort(); // Set TX/RX antenna ports to 1
	gNano.setReadPower(500); // 5.00 dBm. Higher values may cause USB port to brown out
	gNano.setWritePower(500); // 5.00 dBm. Higher values may cause USB port to brown out
	gNano.setRegion(REGION_NORTHAMERICA);
	return (true);
}

void scanForTags()
{
	byte epc[12];
	byte epcLen;
	byte response = gNano.readTagEPC(epc, epcLen, 500);

	if (response == RESPONSE_SUCCESS)
	{
		gNumTags++;
	}
}