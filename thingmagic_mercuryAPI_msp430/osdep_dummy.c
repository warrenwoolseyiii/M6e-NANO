/**
 *  @file osdep_dummy.c
 *  @brief Mercury API - Sample OS-dependent functions that do nothing
 *  @author Nathan Williams
 *  @date 2/24/2010
 */

#include <stdint.h>
#include "osdep.h"

uint32_t tmr_gettime_low()
{
    return 0;
}

uint32_t tmr_gettime_high()
{
    return 0;
}

void tmr_sleep(uint32_t sleepms)
{

}

TMR_TimeStructure
tmr_gettimestructure()
{
  TMR_TimeStructure timestructure;

  timestructure.tm_year = (uint32_t)0;
  timestructure.tm_mon = (uint32_t)0;
  timestructure.tm_mday = (uint32_t)0;
  timestructure.tm_hour = (uint32_t)0;
  timestructure.tm_min = (uint32_t)0;
  timestructure.tm_sec = (uint32_t)0;

  return timestructure;
}
