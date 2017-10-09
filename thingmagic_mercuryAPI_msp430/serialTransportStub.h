/* ========================================================================== *
 *               Copyright (C) Warren Woolsey - All Rights Reserved           *
 *  Unauthorized copying of this file, via any medium is strictly prohibited  *
 *                       Proprietary and confidential.                        *
 * Written by Warren Woolsey                                                  *
 * ========================================================================== */
#ifndef SERIALTRANSPORTSTUB_H_
#define SERIALTRANSPORTSTUB_H_

#include "driverlib/MSP430F5xx_6xx/driverlib.h"

// globally defined functions
uint8_t initSerialTransport(uint32_t baud);
void takeDownSerialTransport();
uint8_t send(uint8_t *data, uint32_t len, const uint64_t timeoutMs);
uint8_t receive(uint8_t *data, uint32_t len, const uint32_t timeoutMs);

#endif /* SERIALTRANSPORTSTUB_H_ */
