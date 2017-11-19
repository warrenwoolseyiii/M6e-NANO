/*
 * timer.h
 *
 * Created: 8/9/2017 12:00:21 PM
 *  Author: warre
 */ 
#ifndef TIMER_H_
#define TIMER_H_

#include "core.h"

#ifdef __cplusplus
extern "C" {
#endif

// special type definitions
typedef enum {
	timer_0,
	timer_1,
	timer_2	
}timer_t;

typedef enum {
	no_source,
	internal_clk,
	ext_clk_falling_edge,
	ext_clk_rising_edge,
	async_clk
}timer_clk_src_t;

typedef enum {
	clk_io_no_clk,
	clk_io_div_by_1,
	clk_io_div_by_8,
	clk_io_div_by_32,
	clk_io_div_by_64,
	clk_io_div_by_128,
	clk_io_div_by_256,
	clk_io_div_by_1024
}timer_clk_divider_t;

typedef enum {
	src_timer_overflow = 0b1,
	src_output_compare_a = 0b10,
	src_output_compare_b = 0b100,
	src_input_capture = 0b10000,
	src_all = 0xff
}timer_interrupt_srcs_t;

typedef enum {
	output_compare_a,
	output_compare_b	
}timer_output_compare_srcs_t;

// globally defined functions
// TODO: PWM
void timer_disableTimer(timer_t timer);
void timer_setExtClkFreq(timer_t timer, uint32_t freq);
timer_error_t timer_enableInterruptSrc(timer_t timer, timer_interrupt_srcs_t src);
timer_error_t timer_disableInterruptSrc(timer_t timer, timer_interrupt_srcs_t src);
void timer_setNewOutputCompareValue(timer_t timer, timer_output_compare_srcs_t ocSrc, uint16_t cntVal);
timer_error_t timer_configureInNormalMode(timer_t timer, timer_clk_src_t clkSrc, timer_clk_divider_t preScaler);
timer_error_t timer_configureInCTCMode(timer_t timer, timer_clk_src_t clkSrc, uint32_t desiredFreq);

#ifdef __cplusplus
}
#endif /* __cplusplus */

#endif /* TIMER_H_ */