/* ========================================================================== *
 *               Copyright (C) Warren Woolsey - All Rights Reserved           *
 *  Unauthorized copying of this file, via any medium is strictly prohibited  *
 *                       Proprietary and confidential.                        *
 * Written by Warren Woolsey                                                  *
 * ========================================================================== */
#ifndef SERIALTRANSPORTSTUB_H_
#define SERIALTRANSPORTSTUB_H_

#include <hal/atmega328p/core/core.h>
#include <hal/atmega328p/uart/uart.h>

#ifdef __cplusplus
extern "C" {
#endif

// globally defined functions
bool_t initSerialTransport(uart_baud_rates_t baud);
void takeDownSerialTransport();
bool_t send(uint8_t *data, uint16_t len);
bool_t receive(uint8_t *data, uint16_t len);

#ifdef __cplusplus
}
#endif /* __cplusplus */


#endif /* SERIALTRANSPORTSTUB_H_ */