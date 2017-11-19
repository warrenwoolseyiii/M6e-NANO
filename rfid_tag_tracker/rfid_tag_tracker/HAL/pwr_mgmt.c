/*
 * pwr_mgmt.c
 *
 * Created: 8/22/2017 12:17:58 PM
 *  Author: warre
 */ 
#include "pwr_mgmt.h"

// macro functions
#define PWR_RST_SLP_MODE (SMCR = 0)
#define PWR_RST_REDUCTION (PRR = 0)

// globally defined functions
void pwr_configureSleepMode(sleep_modes_t mode)
{
	PWR_RST_SLP_MODE;
	SMCR |= mode;
}

void pwr_setPwrReductionToPeripheral(pwr_reduction_t peripheral)
{
	PRR |= peripheral;
}

void pwr_clrPwrReductionToPeripheral(pwr_reduction_t peripheral)
{
	PRR &= ~(peripheral);
}

void pwr_enterSleep(bool_t bodEnabled)
{
	SMCR |= 0b1;
	
	if (!bodEnabled)
	{
		MCUCR |= ((1 << BODS) | (1 << BODSE));
		MCUCR &= ~(1 << BODSE);
	}
	
	asm volatile("sleep");
}

void pwr_exitSleep()
{
	SMCR &= ~(0b1);
}