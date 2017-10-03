/* ========================================================================== *
 *               Copyright (C) Warren Woolsey - All Rights Reserved           *
 *  Unauthorized copying of this file, via any medium is strictly prohibited  *
 *                       Proprietary and confidential.                        *
 * Written by Warren Woolsey                                                  *
 * ========================================================================== */
#include "osDepStub.h"
#include "driverlib/MSP430F5xx_6xx/driverlib.h"

// file specific global variables
static volatile uint64_t gSysUpTimeInMillis = 0;

// locally defined functions
#pragma vector=TIMER0_A0_VECTOR
__interrupt void sysTimerISR (void)
{
    gSysUpTimeInMillis++;
}

void configureSysTimer()
{
    Timer_A_initUpModeParam params;

    params.clockSource = TIMER_A_CLOCKSOURCE_SMCLK;
    params.clockSourceDivider = TIMER_A_CLOCKSOURCE_DIVIDER_1;
    params.timerPeriod = 3999;  // assume SMCLK is 8 MHz -- 4,000,000 / (3,999 + 1) = 1,000
    params.timerInterruptEnable_TAIE = TIMER_A_TAIE_INTERRUPT_DISABLE;
    params.captureCompareInterruptEnable_CCR0_CCIE =
            TIMER_A_CAPTURECOMPARE_INTERRUPT_ENABLE;
    params.timerClear = TIMER_A_DO_CLEAR;
    params.startTimer = true;

    Timer_A_initUpMode(TIMER_A0_BASE, &params);
}

// globally defined functions
uint8_t initOsDepStub()
{
    configureSysTimer();
    return 0;
}

uint64_t getSysUpTimeInMillis()
{
	return gSysUpTimeInMillis;
}

void variableDelayMillis(uint64_t durationInMillis)
{
    uint64_t startTime = gSysUpTimeInMillis;
    while ((gSysUpTimeInMillis - startTime) > durationInMillis)
        __delay_cycles(64); // do this to make sure the loop isn't too tight
}
