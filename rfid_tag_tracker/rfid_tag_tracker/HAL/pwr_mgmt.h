/*
 * pwr_mgmt.h
 *
 * Created: 8/22/2017 12:17:46 PM
 *  Author: warre
 */ 
#ifndef PWR_MGMT_H_
#define PWR_MGMT_H_

#include "core.h"

#ifdef __cplusplus
extern "C" {
#endif

// special type definitions
typedef enum {
	idle = 0,
	adc_noise_reduc = 0b10,
	power_down = 0b100,
	power_save = 0b110,
	standby = 0b1100,
	extended_standby = 0b1110	
}sleep_modes_t;

typedef enum {
	pwr_twi = (1 << PRTWI),
	pwr_timer2 = (1 << PRTIM2),
	pwr_timer0 = (1 << PRTIM0),
	pwr_timer1 = (1 << PRTIM1),
	pwr_spi0 = (1 << PRSPI),
	pwr_uart0 = (1 << PRUSART0),
	pwr_adc = (1 << PRADC),
	pwr_all = 0xff
}pwr_reduction_t;

// globally defined functions
void pwr_configureSleepMode(sleep_modes_t mode);
void pwr_setPwrReductionToPeripheral(pwr_reduction_t peripheral);
void pwr_clrPwrReductionToPeripheral(pwr_reduction_t peripheral);
void pwr_enterSleep(bool_t bodEnabled);
void pwr_exitSleep();

#ifdef __cplusplus
}
#endif /* __cplusplus */

#endif /* PWR_MGMT_H_ */