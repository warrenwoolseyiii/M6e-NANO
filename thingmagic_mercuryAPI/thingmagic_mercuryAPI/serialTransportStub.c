/* ========================================================================== *
 *               Copyright (C) Warren Woolsey - All Rights Reserved           *
 *  Unauthorized copying of this file, via any medium is strictly prohibited  *
 *                       Proprietary and confidential.                        *
 * Written by Warren Woolsey                                                  *
 * ========================================================================== */
#include "serialTransportStub.h"

// globally defined functions
bool_t initSerialTransport(uart_baud_rates_t baud)
{
	uart_params_t params;
	bool_t rtn;

	params.baudRate = baud;
	params.multiProcessorMode = FALSE;
	params.numDataBits = 8;
	params.numStopBits = 1;
	params.parity = no_parity;

	if (uart_configurePeripheral(&params) != uart_no_error)
		rtn = FALSE;
	else
		rtn = TRUE;
	
	return rtn;
}

void takeDownSerialTransport()
{
	uart_deConfigurePeripheral();
}

bool_t send(uint8_t *data, uint16_t len)
{
	uart_transaction_t transac;
	bool_t rtn;

	UART_INIT_TRANSAC_OBJ(transac);
	transac.data = data;
	transac.len = len;

	if (uart_blockingWrite(&transac) != uart_no_error)
		rtn = FALSE;
	else
		rtn = TRUE;

	return rtn;
}

bool_t receive(uint8_t *data, uint16_t len)
{
	uart_transaction_t transac;
	bool_t rtn;

	UART_INIT_TRANSAC_OBJ(transac);
	transac.data = data;
	transac.len = len;

	if (uart_blockingRead(&transac) != uart_no_error)
		rtn = FALSE;
	else
		rtn = TRUE;

	return rtn;
}