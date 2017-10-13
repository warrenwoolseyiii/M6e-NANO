#include <msp430.h>
#include "tm_reader.h"
#include "osDepStub.h"
#include "driverlib/MSP430F5xx_6xx/driverlib.h"

#define F_CPU 16000000UL

void init()
{
    UCS_initClockSignal(UCS_FLLREF, UCS_REFOCLK_SELECT, UCS_CLOCK_DIVIDER_1);
    UCS_initClockSignal(UCS_ACLK, UCS_REFOCLK_SELECT, UCS_CLOCK_DIVIDER_1);
    UCS_initFLLSettle((F_CPU / 1000), (F_CPU / UCS_REFOCLK_FREQUENCY));
}

int16_t main(void)
{
    TMR_Reader r, *rp;
    TMR_Status ret;
    TMR_Region region;

    WDTCTL = WDTPW | WDTHOLD;   // Stop watchdog timer
    __enable_interrupt();

    init();

    initOsDepStub();
    rp = &r;
    ret = TMR_create(rp, "tmr:///dev/ttyS1");
    if (ret != TMR_SUCCESS)
        goto FAIL;
    ret = TMR_connect(rp);  // returning NOT FOUND because the key is wrong.. see serial_reader.c
    if (ret != TMR_SUCCESS)
        goto FAIL;

    FAIL:
    while (1);
}
