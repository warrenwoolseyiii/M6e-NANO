#ifndef UTIL_H_
#define UTIL_H_

#include <stdbool.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

// globally defined functions
bool initMillis();
uint32_t millis();

#ifdef __cplusplus
}
#endif /* __cplusplus */

#endif /* UTIL_H_ */