/*
 * core.c
 *
 * Created: 7/17/2017 10:12:44 AM
 *  Author: warre
 */
#include "core.h"
#include "isr.h"
#include <util/delay.h>

// constants
#define WDT_TIME_OUT_BIT_MASK 0x001ffc00

// macro functions
#define GET_RESET_SOURCE(x) (x = MCUSR)
#define CLR_RESET_SOURCE (MCUSR &= 0)

#define WDT_IS_INT_FLAG_SET (WDTCSR & (1 << WDIF))
#define WDT_CLR_INT_FLAG (WDTCSR |= (1 << WDIF))

#define WDT_CHANGE_EN {\
MCUSR &= ~(1 << WDRF);\
WDTCSR |= (1 << WDCE) | (1 << WDE);\
}

#define WDT_SET_TO_INT_MODE(bits) {\
bits &= ~(1 << WDE);\
bits |= (1 << WDIE);\
WDT_CHANGE_EN;\
WDTCSR = bits;\
}
#define WDT_SET_TO_SYS_RESET_MODE (WDT_CHANGE_EN)
#define WDT_SET_TO_INT_AND_SYS_RESET_MODE {\
WDT_CHANGE_EN;\
WDTCSR |= WDIE;\
}
#define WDT_STOP {\
WDT_CHANGE_EN;\
WDTCSR = 0;\
}

// file specific global variables
static bool_t gGlobalIntsEn = FALSE;
static bool_t gReEnableGlobalInts = FALSE;
static uint8_t gNestedBlockingTransacDepth = 0;

// locally defined functions
wdt_timeout_t core_setWDTPeriod(wdt_timeout_t timeOut)
{
	core_enterBlockingTransaction();
	core_resetWDT();

	switch (timeOut)
	{
		case wdt_16_ms:
			// 2048 cycles
			WDT_CHANGE_EN;
			WDTCSR = 0;
			break;
		case wdt_32_ms:
			// 4096 cycles
			WDT_CHANGE_EN;
			WDTCSR = (1 << WDP0);
		case wdt_64_ms:
			// 8192 cycles
			WDT_CHANGE_EN;
			WDTCSR = (1 << WDP1);
			break;
		case wdt_128_ms:
			// 16384 cycles
			WDT_CHANGE_EN;
			WDTCSR = ((1 << WDP0) | (1 << WDP1));
			break;
		case wdt_256_ms:
			// 32768 cycles
			WDT_CHANGE_EN;
			WDTCSR = (1 << WDP2);
			break;
		case wdt_512_ms:
			// 65536 cycles
			WDT_CHANGE_EN;
			WDTCSR = ((1 << WDP2) | (1 << WDP0));
			break;
		case wdt_1024_ms:
			// 131072 cycles
			WDT_CHANGE_EN;
			WDTCSR = ((1 << WDP2) | (1 << WDP1));
			break;
		case wdt_2048_ms:
			// 262144 cycles
			WDT_CHANGE_EN;
			WDTCSR = ((1 << WDP2) | (1 << WDP1) | (1 << WDP0));
			break;
		case wdt_4096_ms:
			// 524288 cycles
			WDT_CHANGE_EN;
			WDTCSR = (1 << WDP3);
			break;
		case wdt_8192_ms:
			// 1048576 cycles
			WDT_CHANGE_EN;
			WDTCSR = ((1 << WDP3) | (1 << WDP0));
			break;
		default:
			// 2048 cycles
			WDT_CHANGE_EN;
			WDTCSR = 0;
			timeOut = wdt_16_ms;
			break;
	}

	core_exitBlockingTransaction();

	return timeOut;
}

// globally defined functions
void core_disableGlobalInt()
{
    gGlobalIntsEn = FALSE;
	asm volatile("cli");
}

void core_enableGlobalInt()
{
	if (!isr_isHandlerTableInit())
		isr_initHandlerTable();

    gGlobalIntsEn = TRUE;
	asm volatile("sei");
}

bool_t core_areGlobalIntsEn()
{
	return gGlobalIntsEn;
}

reset_types_t core_getResetSource()
{
	uint8_t flags;
	reset_types_t rtn;

	GET_RESET_SOURCE(flags);

	if (flags & (1 << PORF))
		rtn = power_on_reset;
	else if (flags & (1 << EXTRF))
		rtn = external_reset;
	else if (flags & (1 << BORF))
		rtn = brown_out_reset;
	else if (flags & (1 << WDRF))
		rtn = watch_dog_reset;
	else
		rtn = no_reset_source;

	if (rtn != no_reset_source)
		CLR_RESET_SOURCE;

	return rtn;
}

uint32_t core_initWDT(wdt_modes_t mode, wdt_timeout_t timeOut)
{
	uint8_t bits = 0;

	timeOut = core_setWDTPeriod(timeOut);
	bits = WDTCSR;

	switch (mode)
	{
		case wdt_mode_stopped:
			WDT_STOP;
			timeOut = 0;
			break;
		case wdt_mode_int_only:
			WDT_SET_TO_INT_MODE(bits);
			break;
		case wdt_mode_sys_reset_only:
			WDT_SET_TO_SYS_RESET_MODE;
			break;
		case wdt_mode_int_and_sys_reset:
			WDT_SET_TO_INT_AND_SYS_RESET_MODE;
			break;
		default:
			WDT_STOP;
			break;
	}

	return timeOut;
}

void core_stopWDT()
{
	WDT_STOP;
}

void core_resetWDT()
{
	asm volatile("wdr");
}

void core_atomic16BitSFRWrite(volatile uint8_t *regH, volatile uint8_t *regL, uint16_t val)
{
	core_enterBlockingTransaction();
	*regH = ((val >> 8) & 0xff);
	*regL = (val & 0xff);
	core_exitBlockingTransaction();
}

uint16_t core_atomic16BitSFRRead(volatile uint8_t *regH, volatile uint8_t *regL)
{
	uint16_t rtn = 0;

	core_enterBlockingTransaction();
	rtn = *regH;
	rtn <<= 8;
	rtn |= *regL;
	core_exitBlockingTransaction();

	return rtn;
}

void core_enterBlockingTransaction()
{
#ifndef DISABLE_INTERRUPTS_IN_BLOCKING_TRANSACTIONS
    if (gNestedBlockingTransacDepth == 0)
    {
	    gReEnableGlobalInts = gGlobalIntsEn;
	    core_disableGlobalInt();
    }
    
    gNestedBlockingTransacDepth++;
#endif /* DISABLE_INTERRUPTS_IN_BLOCKING_TRANSACTIONS */
}

void core_exitBlockingTransaction()
{
#ifndef DISABLE_INTERRUPTS_IN_BLOCKING_TRANSACTIONS
    gNestedBlockingTransacDepth--;
    if (gNestedBlockingTransacDepth == 0)
    {
	    if (gReEnableGlobalInts)
			core_enableGlobalInt();
    }        
#endif /* DISABLE_INTERRUPTS_IN_BLOCKING_TRANSACTIONS */
}

uint32_t core_roundToNearestInt(float_t val)
{
	uint32_t rtn = (uint32_t)val;
	float_t diff = val - (float_t)rtn;

	if (diff >= 0.5)
		rtn += 1;

	return rtn;
}

void core_delayMSVariable(uint32_t ms)
{
	for (; ms > 0; ms--)
		DELAY_MS_CONST(1);
}

void core_delayUSVariable(uint32_t us)
{
	for (; us > 0; us--)
		DELAY_US_CONST(1);
}

void core_delayCPUCyclesVariable(uint32_t cycles)
{
	for (; cycles > 0; cycles--)
		NOP;
}
