/* ========================================================================== *
 *               Copyright (C) Warren Woolsey - All Rights Reserved           *
 *  Unauthorized copying of this file, via any medium is strictly prohibited  *
 *                       Proprietary and confidential.                        *
 * Written by Warren Woolsey                                                  *
 * ========================================================================== */
#include "serialTransportStub.h"
#include "osDepStub.h"

// globally defined functions
uint8_t initSerialTransport(uint32_t baud)
{
    USCI_A_UART_initParam params;

    if (baud != 115200UL)
        return STATUS_FAIL;

    // TODO: bounce if the clock isn't quite right

    params.selectClockSource = USCI_A_UART_CLOCKSOURCE_SMCLK;
    params.uartMode = USCI_A_UART_MODE;
    params.msborLsbFirst = USCI_A_UART_MSB_FIRST;
    params.parity = USCI_A_UART_NO_PARITY;
    params.numberofStopBits = USCI_A_UART_ONE_STOP_BIT;

    // assumes SMCLK is set to 16 MHz
    params.clockPrescalar = 8;
    params.firstModReg = 11;
    params.secondModReg = 0;
    params.overSampling = USCI_A_UART_OVERSAMPLING_BAUDRATE_GENERATION;

    if (STATUS_FAIL == USCI_A_UART_init(USCI_A0_BASE, &params))
        return STATUS_FAIL;

    USCI_A_UART_enable(USCI_A0_BASE);
    return STATUS_SUCCESS;
}

void takeDownSerialTransport()
{
    USCI_A_UART_disable(USCI_A0_BASE);
}

uint8_t send(uint8_t *data, uint32_t len, const uint64_t timeoutMs)
{
    uint64_t startTime = getSysUpTimeInMillis();
    uint8_t status;

    while ((getSysUpTimeInMillis() - startTime) < timeoutMs)
    {
        status = USCI_A_UART_getInterruptStatus(USCI_A0_BASE, USCI_A_UART_TRANSMIT_INTERRUPT_FLAG);
        if (status == USCI_A_UART_TRANSMIT_INTERRUPT_FLAG)
        {
            USCI_A_UART_transmitData(USCI_A0_BASE, *data);
            data++;
            len--;
            if (len == 0)
                return STATUS_SUCCESS;
        }
    }

    return STATUS_FAIL;
}

uint8_t receive(uint8_t *data, uint32_t len, const uint32_t timeoutMs)
{
    uint64_t startTime = getSysUpTimeInMillis();
    uint8_t status;

    while ((getSysUpTimeInMillis() - startTime) < timeoutMs)
    {
        status = USCI_A_UART_getInterruptStatus(USCI_A0_BASE, USCI_A_UART_RECEIVE_INTERRUPT_FLAG);
        if (status == USCI_A_UART_RECEIVE_INTERRUPT_FLAG)
        {
            *data = USCI_A_UART_receiveData(USCI_A0_BASE);
            data++;
            len--;
            if (len == 0)
                return STATUS_SUCCESS;
        }
    }

    return STATUS_FAIL;
}

