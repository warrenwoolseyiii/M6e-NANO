/*
 * eeprom.c
 *
 * Created: 8/21/2017 3:53:46 PM
 *  Author: warre
 */ 
#include "eeprom.h"

// macro functions
#define EEPROM_VALIDATE_ADDR(addr) ((addr > EEPROM_MAX_ADDR) ? FALSE : TRUE)
#define EEPROM_VALIDATE_LEN(len) ((len > EEPROM_MAX_ADDR) ? FALSE : TRUE)

#define EEPROM_SET_ADDR(addr) core_atomic16BitSFRWrite(&EEARH, &EEARL, addr)

#define EEPROM_SET_DATA(byte) (EEDR = byte)
#define EEPROM_GET_DATA(byte) (byte = EEDR)

#define EEPROM_CLR_MODE (EECR &= ~((1 << EEPM0) | (1 << EEPM1)))
#define EEPROM_ERASE_AND_WRITE_MODE (EEPROM_CLR_MODE)
#define EEPROM_ERASE_MODE {\
EEPROM_CLR_MODE;\
EECR |= (1 << EEPM0);\
}
#define EEPROM_WRITE_MODE {\
EEPROM_CLR_MODE;\
EECR |= (1 << EEPM1);\
}

#define EEPROM_IS_PRGM_RDY (!(EECR & (1 << EEPE)))
#define EEPROM_IS_FLASH_BUSY (!(SPMCSR & (1 << SPMEN)))

// locally defined functions
eeprom_error_t eeprom_validateAddrAndLen(uint16_t addr, uint16_t len)
{
	if (!EEPROM_VALIDATE_ADDR(addr))
		return eeprom_illegal_addr;
	else if (!EEPROM_VALIDATE_LEN(len))
		return eeprom_illegal_len;
		
	return eeprom_no_error;
}

eeprom_error_t eeprom_waitForPgrmRdy()
{
	uint16_t timeOutInMillis = 0;
	
	do
	{
		if ((EEPROM_IS_PRGM_RDY) && (EEPROM_IS_FLASH_BUSY))
			return eeprom_no_error;
		
		DELAY_MS_CONST(1);
		timeOutInMillis++;
	} while (timeOutInMillis < 10);
	
	return eeprom_timeout;
}

uint16_t eeprom_incrementAddr(uint16_t addr)
{
	addr++;
	if (addr > EEPROM_MAX_ADDR)
		addr = 0;
	
	return addr;
}

void eeprom_writeByte(uint16_t addr, uint8_t byte)
{
	uint8_t bits;
	
	// need to set EEMPE and clear EEPE at the same time
	bits = EECR;
	bits &= ~(1 << EEPE);
	bits |= (1 << EEMPE);
	
	// perform the write
	EEPROM_SET_ADDR(addr);
	EEPROM_SET_DATA(byte);
	EECR = bits;
	EECR |= (1 << EEPE);
}

uint8_t eeprom_readByte(uint16_t addr)
{
	uint8_t data;
	
	EEPROM_SET_ADDR(addr);
	EECR |= (1 << EERE);
	EEPROM_GET_DATA(data);
	
	return data;
}

// globally defined functions
eeprom_error_t eeprom_blockingErase(eeprom_transaction_t *transac)
{
	eeprom_error_t rtnCode = eeprom_validateAddrAndLen(transac->addr, transac->len);
	uint16_t i;
	
	if (rtnCode != eeprom_no_error)
		return rtnCode;
	
	EEPROM_ERASE_MODE;
	core_enterBlockingTransaction();
	
	for (i = 0; i < transac->len; i++)
	{
		// wait for the pgrm flag to become available
		rtnCode = eeprom_waitForPgrmRdy();
		if (rtnCode != eeprom_no_error)
			break;
		
		eeprom_writeByte(transac->addr, 0xff);
		transac->addr = eeprom_incrementAddr(transac->addr); 
	}
	
    // wait for the pgrm flag to become available on our way out so we know the 
    // transaction finished
    rtnCode = eeprom_waitForPgrmRdy();
    
	core_exitBlockingTransaction();
		
	return rtnCode;
}

eeprom_error_t eeprom_blockingWrite(eeprom_transaction_t *transac)
{
	eeprom_error_t rtnCode = eeprom_validateAddrAndLen(transac->addr, transac->len);
	uint16_t addr, i;

	if (rtnCode != eeprom_no_error)
		return rtnCode;
	
	addr = transac->addr;

	rtnCode = eeprom_blockingErase(transac);
	if (rtnCode != eeprom_no_error)
		return rtnCode;

	transac->addr = addr;

	EEPROM_WRITE_MODE;
	core_enterBlockingTransaction();
	
	for (i = 0; i < transac->len; i++)
	{
		// wait for the pgrm flag to become available
		rtnCode = eeprom_waitForPgrmRdy();
		if (rtnCode != eeprom_no_error)
			break;
		
		eeprom_writeByte(transac->addr, transac->data[i]);
		transac->addr = eeprom_incrementAddr(transac->addr);
	}
	
    // wait for the pgrm flag to become available on our way out so we know the
    // transaction finished
    rtnCode = eeprom_waitForPgrmRdy();
    
	core_exitBlockingTransaction();
	
	return rtnCode;
}

eeprom_error_t eeprom_blockingRead(eeprom_transaction_t *transac)
{
	eeprom_error_t rtnCode = eeprom_validateAddrAndLen(transac->addr, transac->len);
	uint16_t i;
	
	if (rtnCode != eeprom_no_error)
		return rtnCode;
		
	if ((transac->len > 0) && (transac->data == NULL))
		return eeprom_attempted_to_dereference_null_ptr;
		
	core_enterBlockingTransaction();
	
	for (i = 0; i < transac->len; i++)
	{
		// wait for the pgrm flag to become available
		rtnCode = eeprom_waitForPgrmRdy();
		if (rtnCode != eeprom_no_error)
			break;
			
		transac->data[i] = eeprom_readByte(transac->addr);
		transac->addr = eeprom_incrementAddr(transac->addr);
	}
	
	core_exitBlockingTransaction();
	
	return rtnCode;
}