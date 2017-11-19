/*
 * timer.c
 *
 * Created: 8/9/2017 12:00:40 PM
 *  Author: warre
 */ 
#include "timer.h"

// local constants
#define TIMER2_ASYNC_FLAGS_BIT_MASK 0x1f
#define TIMER2_TCNT2_UB 0x10
#define TIMER2_OCR2A_UB 0x08
#define TIMER2_OCR2B_UB 0x04
#define TIMER2_TCCR2A_UB 0x02
#define TIMER2_TCCR2B_UB 0x01

// macro functions
#define TIMER0_RST {\
TIMSK0 = 0;\
TCCR0A = 0;\
TCCR0B = 0;\
TCNT0 = 0;\
OCR0A = 0;\
OCR0B = 0;\
TIFR0 |= 0x7;\
}
#define TIMER0_SET_CLK_SRC(src) {\
TCCR0B &= ~(0b111);\
TCCR0B |= src;\
}
#define TIMER0_CLR_MODE_BITS {\
TCCR0A &= ~((1 << WGM01) | (1 << WGM00));\
TCCR0B &= ~(1 << WGM02);\
}

#define TIMER1_RST {\
TIMSK1 = 0;\
TCCR1A = 0;\
TCCR1B = 0;\
TCCR1C = 0;\
core_atomic16BitSFRWrite(&TCNT1H, &TCNT1L, 0);\
core_atomic16BitSFRWrite(&OCR1AH, &OCR1AL, 0);\
core_atomic16BitSFRWrite(&OCR1BH, &OCR1BL, 0);\
TIFR1 |= 0b100111;\
}
#define TIMER1_SET_CLK_SRC(src) {\
TCCR1B &= ~(0b111);\
TCCR1B |= src;\
}
#define TIMER1_CLR_MODE_BITS {\
TCCR1A &= ~((1 << WGM11) | (1 << WGM10));\
TCCR1B &= ~((1 << WGM13) | (1 << WGM12));\
}

#define TIMER2_RST {\
TIMSK2 = 0;\
ASSR = 0;\
TCCR2A = 0;\
TCCR2B = 0;\
TCNT2 = 0;\
OCR2A = 0;\
OCR2B = 0;\
TIFR2 |= 0x7;\
}
#define TIMER2_SET_CLK_SRC(src) {\
TCCR2B &= ~(0b111);\
TCCR2B |= src;\
}
#define TIMER2_CLR_MODE_BITS {\
TCCR2A &= ~((1 << WGM21) | (1 << WGM20));\
TCCR2B &= ~(1 << WGM22);\
}
#define TIMER2_GET_ASYNC_UPDATE_FLAGS(bits) (bits = ASSR & TIMER2_ASYNC_FLAGS_BIT_MASK)

// locally defined type definitions
typedef struct {
	timer_clk_divider_t preScaler;
	uint16_t cntr;
	uint8_t clkSelBits;
}timer_params_t;

// file specific global variables
static uint32_t gExtClkFreq0 = 0;
static uint32_t gExtClkFreq1 = 0;
static uint32_t gExtClkFreq2 = 0;

// locally defined functions
int8_t timer_getPreScalerBits(timer_t timer, timer_clk_divider_t divider)
{
	int8_t rtn = 0;
	
	switch (divider)
	{
		case clk_io_no_clk:
			rtn = 0;
			break;
		case clk_io_div_by_1:
			rtn = 0b1;
			break;
		case clk_io_div_by_8:
			rtn = 0b10;
			break;
		case clk_io_div_by_32:
			if (timer == timer_2)
				rtn = 0b11;
			else
				rtn = -1;
			break;
		case clk_io_div_by_64:
			if (timer == timer_2)
				rtn = 0b100;
			else
				rtn = 0b11;
			break;
		case clk_io_div_by_128:
			if (timer == timer_2)
				rtn = 0b101;
			else
				rtn = -1;
			break;
		case clk_io_div_by_256:
			if (timer == timer_2)
				rtn = 0b110;
			else
				rtn = 0b100;
			break;
		case clk_io_div_by_1024:
			if (timer == timer_2)
				rtn = 0b111;
			else
				rtn = 0b101;
			break;
	}
	
	return rtn;
}

timer_params_t timer_calcPreScalerAndCntr(uint32_t srcClkFreq, timer_t timer, uint32_t freq)
{
	timer_params_t rtn;
	float_t tmpCntr;
	uint32_t calcCntr = 0;
	uint16_t availablePreScalers[8];
	uint16_t maxCntrVal = 0;
	uint8_t maxNumPreScalers = 8;
	uint8_t i;
	
	// check if the frequency is acceptable
	if ((srcClkFreq / freq) < 2)
	{
		rtn.preScaler = clk_io_no_clk;
		rtn.cntr = 0;
		rtn.clkSelBits = 0;
		return rtn;
	}
	
	// determine the max counter value
	if (timer != timer_1)
		maxCntrVal = 0xff;
	else
		maxCntrVal = 0xffff;
	
	// determine how many pre scalers we can have
	if (timer != timer_2)
	{
		maxNumPreScalers = 5;
		availablePreScalers[0] = 1;
		availablePreScalers[1] = 8;
		availablePreScalers[2] = 64;
		availablePreScalers[3] = 256;
		availablePreScalers[4] = 1024;
	}
	else
	{
		maxNumPreScalers = 7;
		availablePreScalers[0] = 1;
		availablePreScalers[1] = 8;
		availablePreScalers[2] = 32;
		availablePreScalers[3] = 64;
		availablePreScalers[4] = 128;
		availablePreScalers[5] = 256;
		availablePreScalers[6] = 1024;
	}
	
	for (i = 0; i < maxNumPreScalers; i++)
	{
		tmpCntr = ((float_t)((float_t)srcClkFreq / (float_t)freq) * (1.0 / (2.0 * (float_t)availablePreScalers[i])) - 1.0);
		
		if (tmpCntr >= 0.0)
		{
			calcCntr = (uint32_t)tmpCntr;
			if (calcCntr <= maxCntrVal)
				break;
		}
	}
	
	rtn.cntr = (uint16_t)(calcCntr & 0xffff);
	
	// requested a frequency too low for the selected timer
	if (i >= maxNumPreScalers)
	{
		rtn.preScaler = clk_io_no_clk;
	}
	else
	{
		switch (availablePreScalers[i])
		{
			case 1:
				rtn.preScaler = clk_io_div_by_1;
				break;
			case 8:
				rtn.preScaler = clk_io_div_by_8;
				break;
			case 32:
				rtn.preScaler = clk_io_div_by_32;
				break;
			case 64:
				rtn.preScaler = clk_io_div_by_64;
				break;
			case 128:
				rtn.preScaler = clk_io_div_by_128;
				break;
			case 256:
				rtn.preScaler = clk_io_div_by_256;
				break;
			case 1024:
				rtn.preScaler = clk_io_div_by_1024;
				break;
		}
	}
	
	rtn.clkSelBits = timer_getPreScalerBits(timer, rtn.preScaler);
	
	return rtn;
}

uint16_t timer_calcCntrForEXTClk(timer_t timer, uint32_t freq)
{
	uint32_t calcCntr = 0;
	uint16_t maxCntVal;
	
	switch (timer)
	{
		case timer_0:
			maxCntVal = 0xff;
			calcCntr = gExtClkFreq0 / freq;
			if (calcCntr > maxCntVal)
				calcCntr = maxCntVal;
			break;
		case timer_1:
			maxCntVal = 0xffff;
			calcCntr = gExtClkFreq1 / freq;
			if (calcCntr > maxCntVal)
				calcCntr = maxCntVal;
			break;
		case timer_2:
			maxCntVal = 0xff;
			calcCntr = gExtClkFreq2 / freq;
			if (calcCntr > maxCntVal)
				calcCntr = maxCntVal;
	}
	
	return (uint16_t)(calcCntr & 0xffff);
}

// globally defined functions
// TODO: PWM mode
void timer_disableTimer(timer_t timer)
{
	switch (timer)
	{
		case timer_0:
			TIMER0_RST;
			break;
		case timer_1:
			TIMER1_RST;
			break;
		case timer_2:
			TIMER2_RST;
			break;
	}
}

void timer_setExtClkFreq(timer_t timer, uint32_t freq)
{
	switch (timer)
	{
		case timer_0:
			gExtClkFreq0 = freq;
			break;
		case timer_1:
			gExtClkFreq1 = freq;
			break;
		case timer_2:
			gExtClkFreq2 = freq;
			break;
	}
}

timer_error_t timer_enableInterruptSrc(timer_t timer, timer_interrupt_srcs_t src)
{
	switch (timer)
	{
		case timer_0:
			if (src == src_input_capture)
				return selected_functionality_not_supported_on_timer;
			else
				TIMSK0 |= src;
			break;
		case timer_1:
			TIMSK1 |= src;
			break;
		case timer_2:
			if (src == src_input_capture)
				return selected_functionality_not_supported_on_timer;
			else
				TIMSK2 |= src;
			break;
	}
	
	return timer_no_error;
}

timer_error_t timer_disableInterruptSrc(timer_t timer, timer_interrupt_srcs_t src)
{
	switch (timer)
	{
		case timer_0:
			if (src == src_input_capture)
				return selected_functionality_not_supported_on_timer;
			else
				TIMSK0 &= ~(src);
			break;
		case timer_1:
			TIMSK1 &= ~(src);
			break;
		case timer_2:
			if (src == src_input_capture)
				return selected_functionality_not_supported_on_timer;
			else
				TIMSK2 &= ~(src);
			break;
	}
	
	return timer_no_error;
}

void timer_setNewOutputCompareValue(timer_t timer, timer_output_compare_srcs_t ocSrc, uint16_t cntVal)
{
	switch (timer)
	{
		case timer_0:
			if (ocSrc == output_compare_a)
				OCR0A = (uint8_t)(cntVal & 0xff);
			else if (ocSrc == output_compare_b)
				OCR0B = (uint8_t)(cntVal & 0xff);
			break;
		case timer_1:
			if (ocSrc == output_compare_a)
				core_atomic16BitSFRWrite(&OCR1AH, &OCR1AL, cntVal);
			else if (ocSrc == output_compare_b)
				core_atomic16BitSFRWrite(&OCR1BH, &OCR1BL, cntVal);
			break;
		case timer_2:
			if (ocSrc == output_compare_a)
				OCR2A = (uint8_t)(cntVal & 0xff);
			else if (ocSrc == output_compare_b)
				OCR2B = (uint8_t)(cntVal & 0xff);
			break;
	}
}

timer_error_t timer_configureInNormalMode(timer_t timer, timer_clk_src_t clkSrc, timer_clk_divider_t preScaler)
{
	int8_t clkSelBits = 0;
	
	if (clkSrc == ext_clk_falling_edge && timer != timer_2)
		clkSelBits = 0b110;
	else if (clkSrc == ext_clk_rising_edge && timer != timer_2)
		clkSelBits = 0b111;
	else if (timer == timer_2 && (clkSrc == ext_clk_falling_edge || clkSrc == ext_clk_rising_edge))
		return selected_clk_src_not_supported_on_timer;
	else if (timer != timer_2 && clkSrc == async_clk)
		return selected_clk_src_not_supported_on_timer;
	else if (timer == timer_2 && clkSrc == async_clk && gExtClkFreq2 == 0)
		return external_clk_freq_not_set;
	else
		clkSelBits = timer_getPreScalerBits(timer, preScaler);
		
	if (clkSelBits == -1)
		return selected_clk_src_not_supported_on_timer;
	
	switch (timer)
	{
		case timer_0:
			TIMER0_RST;
			TIMER0_SET_CLK_SRC(clkSelBits);
			break;
		case timer_1:
			TIMER1_RST;
			TIMER1_SET_CLK_SRC(clkSelBits);
			break;
		case timer_2:
			TIMER2_RST;
			if (clkSrc == async_clk)
			{
				// if the async clock is not a 32768 crystal the exclk bit must be set
				if (gExtClkFreq2 != 32768)
					ASSR |= (1 << EXCLK);
				
				ASSR |= (1 << AS2);
				TCCR2A = 0;
				TCCR2B = 0;
				OCR2A = 0;
				OCR2B = 0;
			}
			TIMER2_SET_CLK_SRC(clkSelBits);
			break;
	}
	
	return timer_no_error;
}

timer_error_t timer_configureInCTCMode(timer_t timer, timer_clk_src_t clkSrc, uint32_t desiredFreq)
{
	timer_params_t params;
	
	if (clkSrc == internal_clk)
	{
		// set the counter and the clock select bits
		params = timer_calcPreScalerAndCntr(F_CPU, timer, desiredFreq);
		
		// ensure that the desired frequency is possible to generate
		if (params.preScaler == clk_io_no_clk)
			return requested_freq_out_of_range;
	}
	else if ((timer != timer_2) && (clkSrc == ext_clk_falling_edge || clkSrc == ext_clk_rising_edge))
	{
		// ensure that the external frequency is actually set
		if ((timer == timer_0) && (gExtClkFreq0 == 0))
			return external_clk_freq_not_set;
		else if ((timer == timer_1) && (gExtClkFreq1 == 0))
			return external_clk_freq_not_set;
		
		// set the counter and the clock select bits
		params.cntr = timer_calcCntrForEXTClk(timer, desiredFreq);
		if (clkSrc == ext_clk_falling_edge)
			params.clkSelBits = 0b110;
		else
			params.clkSelBits = 0b111;
	}
	else if ((timer == timer_2) && (clkSrc == async_clk))
	{
		if (gExtClkFreq2 == 0)
			return external_clk_freq_not_set;
			
		params = timer_calcPreScalerAndCntr(gExtClkFreq2, timer, desiredFreq);
		
		// ensure that the desired frequency is possible to generate
		if (params.preScaler == clk_io_no_clk)
			return requested_freq_out_of_range;
	}
	else
	{
		return selected_functionality_not_supported_on_timer;
	}
	
	switch (timer)
	{
		case timer_0:
			TIMER0_RST;
			OCR0A = (uint8_t)(params.cntr & 0xff);
			TIMER0_SET_CLK_SRC(params.clkSelBits);
			TCCR0A |= (1 << WGM01);
			break;
		case timer_1:
			TIMER1_RST;
			core_atomic16BitSFRWrite(&OCR1AH, &OCR1AL, params.cntr);
			TIMER1_SET_CLK_SRC(params.clkSelBits);
			TCCR1B |= (1 << WGM12);
			break;
		case timer_2:
			TIMER2_RST;
			if (clkSrc == async_clk)
			{
				// if the async clock is not a 32768 crystal the exclk bit must be set
				if (gExtClkFreq2 != 32768)
					ASSR |= (1 << EXCLK);
				
				ASSR |= (1 << AS2);
				TCCR2A = 0;
				TCCR2B = 0;
				OCR2A = 0;
				OCR2B = 0;
			}
			
			TIMER2_SET_CLK_SRC(params.clkSelBits);
			OCR2A = (uint8_t)(params.cntr & 0xff);
			TCCR2A |= (1 << WGM21);
			break;
	}
	
	return timer_no_error;
}