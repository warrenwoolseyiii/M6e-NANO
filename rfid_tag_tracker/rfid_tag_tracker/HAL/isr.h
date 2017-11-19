/*
 * isr.h
 *
 * Created: 8/1/2017 4:25:36 PM
 *  Author: warre
 */ 
#ifndef ISR_H_
#define ISR_H_

#include "core.h"

#ifdef __cplusplus
extern "C" {
#endif

// globally defined functions
bool_t isr_isHandlerTableInit();
void isr_initHandlerTable();
void isr_registerHandlerFunc(uint8_t vectorNumber, void (*func) ());
void isr_unRegisterHandlerFunc(uint8_t vectorNumber);

#ifdef __cplusplus
}
#endif /* __cplusplus */

#endif /* ISR_H_ */