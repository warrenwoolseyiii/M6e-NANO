#include <msp430.h>
#include "tm_reader.h"
#include "osDepStub.h"

int main(void)
{
    TMR_Reader r, *rp;
    TMR_Status ret;
    TMR_Region region;

    WDTCTL = WDTPW | WDTHOLD;   // Stop watchdog timer
    __enable_interrupt();

    initOsDepStub();
    rp = &r;
    ret = TMR_create(rp, "eapi:///COM2");
    ret = TMR_connect(rp);
    if (ret != TMR_SUCCESS)
        return 0;

    while (1);
}
