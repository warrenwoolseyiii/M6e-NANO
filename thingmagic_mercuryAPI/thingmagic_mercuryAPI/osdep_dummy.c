/**
 *  @file osdep_dummy.c
 *  @brief Mercury API - Sample OS-dependent functions that do nothing
 *  @author Nathan Williams
 *  @date 2/24/2010
 */

#include <stdint.h>
#include "osdep.h"
#include "osDepStub.h"

uint32_t tmr_gettime_low()
{
	uint64_t millis = getSysUpTimeInMillis();
  return (millis & 0xFFFFFFFF);
}

uint32_t tmr_gettime_high()
{
  uint64_t millis = getSysUpTimeInMillis();
  return ((millis >> 32) & 0xFFFFFFFF);
}

void tmr_sleep(uint32_t sleepms)
{
	variableDelayMillis((uint64_t)sleepms);
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
