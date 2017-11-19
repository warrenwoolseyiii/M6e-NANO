/*
  Library for controlling the Nano M6E from ThingMagic
  This is a stripped down implementation of the Mercury API from ThingMagic

  By: Nathan Seidle @ SparkFun Electronics
  Date: October 3rd, 2016
  https://github.com/sparkfun/Simultaneous_RFID_Tag_Reader

  License: Open Source MIT License
  If you use this code please consider buying an awesome board from SparkFun. It's a ton of
  work (and a ton of fun!) to put these libraries together and we want to keep making neat stuff!
  https://opensource.org/licenses/MIT

  The above copyright notice and this permission notice shall be included in all copies or 
  substantial portions of the Software.
*/

#include "UARTBus.h"
#include <stdbool.h>

#define MAX_MSG_SIZE UART_BUFFER_LEN

#define TMR_SR_OPCODE_VERSION 0x03
#define TMR_SR_OPCODE_SET_BAUD_RATE 0x06
#define TMR_SR_OPCODE_READ_TAG_ID_SINGLE 0x21
#define TMR_SR_OPCODE_READ_TAG_ID_MULTIPLE 0x22
#define TMR_SR_OPCODE_WRITE_TAG_ID 0x23
#define TMR_SR_OPCODE_WRITE_TAG_DATA 0x24
#define TMR_SR_OPCODE_KILL_TAG 0x26
#define TMR_SR_OPCODE_READ_TAG_DATA 0x28
#define TMR_SR_OPCODE_CLEAR_TAG_ID_BUFFER 0x2A
#define TMR_SR_OPCODE_MULTI_PROTOCOL_TAG_OP 0x2F
#define TMR_SR_OPCODE_GET_READ_TX_POWER 0x62
#define TMR_SR_OPCODE_GET_WRITE_TX_POWER 0x64
#define TMR_SR_OPCODE_GET_POWER_MODE 0x68
#define TMR_SR_OPCODE_GET_READER_OPTIONAL_PARAMS 0x6A
#define TMR_SR_OPCODE_GET_PROTOCOL_PARAM 0x6B
#define TMR_SR_OPCODE_SET_ANTENNA_PORT 0x91
#define TMR_SR_OPCODE_SET_TAG_PROTOCOL 0x93
#define TMR_SR_OPCODE_SET_READ_TX_POWER 0x92
#define TMR_SR_OPCODE_SET_WRITE_TX_POWER 0x94
#define TMR_SR_OPCODE_SET_REGION 0x97
#define TMR_SR_OPCODE_SET_READER_OPTIONAL_PARAMS 0x9A
#define TMR_SR_OPCODE_SET_PROTOCOL_PARAM 0x9B

#define COMMAND_TIME_OUT  2000 //Number of ms before stop waiting for response from module

//Define all the ways functions can return
#define ALL_GOOD                        0
#define ERROR_COMMAND_RESPONSE_TIMEOUT  1
#define ERROR_CORRUPT_RESPONSE          2
#define ERROR_WRONG_OPCODE_RESPONSE     3
#define ERROR_UNKNOWN_OPCODE            4
#define RESPONSE_IS_TEMPERATURE         5
#define RESPONSE_IS_KEEPALIVE           6
#define RESPONSE_IS_TEMPTHROTTLE        7
#define RESPONSE_IS_TAGFOUND            8
#define RESPONSE_IS_NOTAGFOUND          9
#define RESPONSE_IS_UNKNOWN             10
#define RESPONSE_SUCCESS  			    11
#define RESPONSE_FAIL          			12

//Define the allowed regions - these set the internal freq of the module
#define REGION_INDIA        0x04
#define REGION_JAPAN        0x05
#define REGION_CHINA        0x06
#define REGION_EUROPE       0x08
#define REGION_KOREA        0x09
#define REGION_AUSTRALIA    0x0B
#define REGION_NEWZEALAND   0x0C
#define REGION_NORTHAMERICA 0x0D
#define REGION_OPEN         0xFF

class RFID
{
// variables
public:
private:
	UARTBus _uartBus0;
	uint8_t _nanoRxBuff[UART_BUFFER_LEN];
	uint16_t _nanoRxBuffNdx;

// methods
public:
	RFID();

	void setBaud( uint32_t baudRate );
	void getVersion();
	void setReadPower( int16_t powerSetting );
	void getReadPower();
	void setWritePower( int16_t powerSetting );
	void getWritePower();
	void setRegion( uint8_t region );
	void setAntennaPort();
	void setAntennaSearchList();
	void setTagProtocol( uint8_t protocol = 0x05 );

	void startContinuousRead(); //Disable filtering and start reading continuously
	void stopContinuousRead(); //Stops continuous read. Give 1000 to 2000ms for the module to stop reading.

	void enableReadFilter();
	void disableReadFilter();

	void setReaderConfiguration( uint8_t option1, uint8_t option2 );
	void getOptionalParameters( uint8_t option1, uint8_t option2 );
	void setProtocolParameters();
	void getProtocolParameters( uint8_t option1, uint8_t option2 );

	uint8_t parseResponse( uint8_t *msg );
	uint8_t getNumTagEPCBytes();
	uint8_t getNumTagDataBytes();
	uint16_t getTagTimestamp();
	uint32_t getTagFreq();
	int8_t getTagRSSI();
    
	bool checkForNewMessage();

	uint8_t readTagEPC( uint8_t *epc, uint8_t &epcLength, uint16_t timeOut = COMMAND_TIME_OUT );
	uint8_t writeTagEPC( char *newID, uint8_t newIDLength, uint16_t timeOut = COMMAND_TIME_OUT );

	uint8_t readData(uint8_t bank, uint32_t address, uint8_t *dataRead, uint8_t &dataLengthRead, uint16_t timeOut = COMMAND_TIME_OUT);
	uint8_t writeData(uint8_t bank, uint32_t address, uint8_t *dataToRecord, uint8_t dataLengthToRecord, uint16_t timeOut = COMMAND_TIME_OUT);

	uint8_t readUserData(uint8_t *userData, uint8_t &userDataLength, uint16_t timeOut = COMMAND_TIME_OUT);
	uint8_t writeUserData(uint8_t *userData, uint8_t userDataLength, uint16_t timeOut = COMMAND_TIME_OUT);

	uint8_t readKillPW(uint8_t *password, uint8_t &passwordLength, uint16_t timeOut = COMMAND_TIME_OUT);
	uint8_t writeKillPW(uint8_t *password, uint8_t passwordLength, uint16_t timeOut = COMMAND_TIME_OUT);

	uint8_t readAccessPW(uint8_t *password, uint8_t &passwordLength, uint16_t timeOut = COMMAND_TIME_OUT);
	uint8_t writeAccessPW(uint8_t *password, uint8_t passwordLength, uint16_t timeOut = COMMAND_TIME_OUT);

	uint8_t readTID(uint8_t *tid, uint8_t &tidLength, uint16_t timeOut = COMMAND_TIME_OUT);

	uint8_t killTag(uint8_t *password, uint8_t passwordLength, uint16_t timeOut = COMMAND_TIME_OUT);

	bool sendMessage(uint8_t opcode, uint8_t *data = 0, uint8_t size = 0, uint32_t timeOut = COMMAND_TIME_OUT, bool waitForResponse = true);
	uint16_t calculateCRC(uint8_t *u8Buf, uint8_t len);
};
