/* ========================================================================== *
 *               Copyright (C) Warren Woolsey - All Rights Reserved           *
 *  Unauthorized copying of this file, via any medium is strictly prohibited  *
 *                       Proprietary and confidential.                        *
 * Written by Warren Woolsey                                                  *
 * ========================================================================== */
#ifndef OSDEPSTUB_H_
#define OSDEPSTUB_H_

#include <hal/atmega328p/core/core.h>

#ifdef __cplusplus
extern "C" {
#endif

// globally defined functions
bool_t initOsDepStub();
uint64_t getSysUpTimeInMillis();
void variableDelayMillis(uint64_t durationInMillis);

#ifdef __cplusplus
}
#endif /* __cplusplus */

#endif /* OSDEPSTUB_H_ */