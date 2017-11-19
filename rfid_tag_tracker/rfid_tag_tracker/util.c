#include "util.h"
#include "HAL/timer.h"
#include "HAL/isr.h"

static volatile uint32_t _millis = 0;

// locally defined functions
void millisISR()
{
	_millis++;
}

// globally defined functions
bool initMillis()
{
	isr_registerHandlerFunc(TIMER1_COMPA_vect_num, millisISR);
	return (timer_configureInCTCMode( timer_1, internal_clk, 1000 ) == timer_no_error);
}

uint32_t millis()
{
	return _millis;
}