/*
 * core.h
 *
 * Created: 7/17/2017 10:13:03 AM
 *  Author: warre
 */
#ifndef CORE_H_
#define CORE_H_

#ifdef __cplusplus
extern "C" {
#endif

#include "types.h"
#include "errorCodes.h"
#include <avr/io.h>
#include <stddef.h>

// global defines and constants
#ifndef F_CPU
#define F_CPU 1000000UL
#endif /* F_CPU */

#ifndef TRUE
#define TRUE 1
#endif /* TRUE */

#ifndef FALSE
#define FALSE 0
#endif /* FLASE */

#include <util/delay.h>

// macro functions
#define DELAY_MS_CONST(x) (_delay_ms(x))
#define DELAY_US_CONST(x) (_delay_us(x))
#define DELAY_CPU_CYCLES_CONST(x) (_delay_loop_2(x))
#define NOP DELAY_CPU_CYCLES_CONST(1)

// special type definitions
typedef enum {
	power_on_reset,
	external_reset,
	brown_out_reset,
	watch_dog_reset,
	no_reset_source
}reset_types_t;

typedef enum {
	wdt_mode_stopped,
	wdt_mode_int_only,
	wdt_mode_sys_reset_only,
	wdt_mode_int_and_sys_reset
}wdt_modes_t;

typedef enum {
	wdt_16_ms = 2048,
	wdt_32_ms = 4096,
	wdt_64_ms = 8192,
	wdt_128_ms = 16384,
	wdt_256_ms = 32768,
	wdt_512_ms = 65536,
	wdt_1024_ms = 131072,
	wdt_2048_ms = 262144,
	wdt_4096_ms = 524288,
	wdt_8192_ms = 1048576
}wdt_timeout_t;

// globally defined functions
void core_disableGlobalInt();
void core_enableGlobalInt();
bool_t core_areGlobalIntsEn();
reset_types_t core_getResetSource();
uint32_t core_initWDT(wdt_modes_t mode, wdt_timeout_t timeOut);
void core_stopWDT();
void core_resetWDT();
void core_atomic16BitSFRWrite(volatile uint8_t *regH, volatile uint8_t *regL, uint16_t val);
uint16_t core_atomic16BitSFRRead(volatile uint8_t *regH, volatile uint8_t *regL);
void core_enterBlockingTransaction();
void core_exitBlockingTransaction();
uint32_t core_roundToNearestInt(float_t val);
void core_delayMSVariable(uint32_t ms);
void core_delayUSVariable(uint32_t us);
void core_delayCPUCyclesVariable(uint32_t cycles);

#ifdef __cplusplus
}
#endif /* __cplusplus */

#endif /* CORE_H_ */
