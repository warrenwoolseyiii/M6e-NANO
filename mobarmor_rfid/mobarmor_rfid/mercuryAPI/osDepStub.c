/* ========================================================================== *
 *               Copyright (C) Warren Woolsey - All Rights Reserved           *
 *  Unauthorized copying of this file, via any medium is strictly prohibited  *
 *                       Proprietary and confidential.                        *
 * Written by Warren Woolsey                                                  *
 * ========================================================================== */
#include "osDepStub.h"

#include <hal/atmega328p/timer/timer.h>
#include <hal/atmega328p/isr/isr.h>

// file specific global variables
static uint64_t gSysUpTimeInMillis = 0;

// locally defined functions
void sysTimerISR()
{
	gSysUpTimeInMillis++;
}

bool_t configureSysTimer()
{
	timer_error_t errorCode;
	
	errorCode = timer_configureInCTCMode(timer_1, internal_clk, 500);
	if (errorCode == timer_no_error)
	{
		isr_registerHandlerFunc(TIMER1_COMPA_vect_num, sysTimerISR);
		timer_enableInterruptSrc(timer_1, src_output_compare_a);
		return TRUE;
	}

	return FALSE;
}

// globally defined functions
bool_t initOsDepStub()
{
	bool_t rtn = TRUE;

	rtn &= configureSysTimer();

	return rtn;
}

uint64_t getSysUpTimeInMillis()
{
	return gSysUpTimeInMillis;
}

void variableDelayMillis(uint64_t durationInMillis)
{
	uint64_t startTime = gSysUpTimeInMillis;
	while ((gSysUpTimeInMillis - startTime) > durationInMillis)
		DELAY_MS_CONST(1);
}