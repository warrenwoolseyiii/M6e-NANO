/*
 * gpio.h
 *
 * Created: 7/27/2017 5:20:16 PM
 *  Author: warre
 */
#ifndef GPIO_H_
#define GPIO_H_

#include "core.h"

#ifdef __cplusplus
extern "C" {
#endif

// special type definitions
typedef enum {
	input,
	output,
	highZ
}gpio_pin_mode_t;

typedef enum {
	port_b,
	port_c,
	port_d
}gpio_ports_t;

typedef enum {
	pin_0 = 0,
	pin_1 = 1,
	pin_2 = 2,
	pin_3 = 3,
	pin_4 = 4,
	pin_5 = 5,
	pin_6 = 6,
	pin_7 = 7
}gpio_pin_t;

typedef enum
{
	ext_int_0,
	ext_int_1
}ext_int_t;

typedef enum
{
	int_req_logic_low,
	int_req_logic_any,
	int_req_falling_edge,
	int_req_rising_edge
}ext_int_mode_t;

typedef struct {
	gpio_ports_t port;
	gpio_pin_t pin;
}gpio_t;

// exposed function prototypes
void gpio_setMode(gpio_t *gpio, gpio_pin_mode_t mode);
void gpio_PCINTConfig(gpio_t *gpio);
void gpio_PCINTDeconfig(gpio_t *gpio);
void gpio_PCINTEnableIRQ(gpio_t *gpio);
void gpio_PCINTDisableIRQ(gpio_t *gpio);
void gpio_EXTINTConfig(ext_int_t intPin, ext_int_mode_t mode);
void gpio_EXTINTDeconfig(ext_int_t intPin);
void gpio_EXTINTEnableIRQ(ext_int_t intPin);
void gpio_EXTINTDisableIRQ(ext_int_t intPin);
void gpio_outputLogicHigh(gpio_t *gpio);
void gpio_outputLogicLow(gpio_t *gpio);
void gpio_outputLogic(gpio_t *gpio, bool_t setLogicHigh);
uint8_t gpio_readInputLogicLevel(gpio_t *gpio);

#ifdef __cplusplus
}
#endif /* __cplusplus */

#endif /* GPIO_H_ */
