/*
 * gpio.c
 *
 * Created: 7/27/2017 5:20:04 PM
 *  Author: warre
 */
#include "gpio.h"

// local constants
#define EXT_INT_0_MODE_BITMASK 0b11
#define EXT_INT_1_MODE_BITMASK 0b1100

// macro functions
#define SET_PORT_B_OUTMODE(pin) (DDRB |= (1 << pin))
#define CLR_PORT_B_OUTMODE(pin) (DDRB &= ~(1 << pin))
#define SET_PORT_B_PINOUT(pin) (PORTB |= (1 << pin))
#define CLR_PORT_B_PINOUT(pin) (PORTB &= ~(1 << pin))
#define READ_PORT_B_INPUT(val, pin) (val = (PINB & (1 << pin)))

#define SET_PORT_C_OUTMODE(pin) (DDRC |= (1 << pin))
#define CLR_PORT_C_OUTMODE(pin) (DDRC &= ~(1 << pin))
#define SET_PORT_C_PINOUT(pin) (PORTC |= (1 << pin))
#define CLR_PORT_C_PINOUT(pin) (PORTC &= ~(1 << pin))
#define READ_PORT_C_INPUT(val, pin) (val = (PINC & (1 << pin)))

#define SET_PORT_D_OUTMODE(pin) (DDRD |= (1 << pin))
#define CLR_PORT_D_OUTMODE(pin) (DDRD &= ~(1 << pin))
#define SET_PORT_D_PINOUT(pin) (PORTD |= (1 << pin))
#define CLR_PORT_D_PINOUT(pin) (PORTD &= ~(1 << pin))
#define READ_PORT_D_INPUT(val, pin) (val = (PIND & (1 << pin)))

#define IS_PCINT0_ENABLED (PCICR & (1 << PCIE0))
#define ENABLE_PCINT0_INT (PCICR |= (1 << PCIE0))
#define DISABLE_PCINT0_INT (PCICR &= ~(1 << PCIE0))
#define CONFIGURE_PCINT0_PIN_FOR_INT(pin) (PCMSK0 |= (1 << pin))
#define DECONFIGURE_PCINT0_PIN_FOR_INT(pin) (PCMSK0 &= ~(1 << pin))

#define IS_PCINT1_ENABLED (PCICR & (1 << PCIE1))
#define ENABLE_PCINT1_INT (PCICR |= (1 << PCIE1))
#define DISABLE_PCINT1_INT (PCICR &= ~(1 << PCIE1))
#define CONFIGURE_PCINT1_PIN_FOR_INT(pin) (PCMSK1 |= (1 << pin))
#define DECONFIGURE_PCINT1_PIN_FOR_INT(pin) (PCMSK1 &= ~(1 << pin))

#define IS_PCINT2_ENABLED (PCICR & (1 << PCIE2))
#define ENABLE_PCINT2_INT (PCICR |= (1 << PCIE2))
#define DISABLE_PCINT2_INT (PCICR &= ~(1 << PCIE2))
#define CONFIGURE_PCINT2_PIN_FOR_INT(pin) (PCMSK2 |= (1 << pin))
#define DECONFIGURE_PCINT2_PIN_FOR_INT(pin) (PCMSK2 &= ~(1 << pin))

#define EXT_INT_SRC_LOGIC_LOW(bits) (bits = 0b00)
#define EXT_INT_SRC_LOGIC_ANY(bits) (bits = 0b01)
#define EXT_INT_SRC_FALLING(bits) (bits = 0b10)
#define EXT_INT_SRC_RISING(bits) (bits = 0b11)

#define ENABLE_EXT_INT_PIN_0 (EIMSK |= (1 << INT0))
#define DISABLE_EXT_INT_PIN_0 (EIMSK &= ~(1 << INT0))

#define ENABLE_EXT_INT_PIN_1 (EIMSK |= (1 << INT1))
#define DISABLE_EXT_INT_PIN_1 (EIMSK &= ~(1 << INT1))

// globally defined functions
void gpio_setMode(gpio_t *gpio, gpio_pin_mode_t mode)
{
	switch (gpio->port)
	{
		case port_b:
			if (mode == highZ)
			{
				CLR_PORT_B_OUTMODE(gpio->pin);
				CLR_PORT_B_PINOUT(gpio->pin);
			}
			else if (mode == input)
			{
				// enter tri-state high-z as an intermediate, see atmega328p gpio data sheet for more information
				CLR_PORT_B_OUTMODE(gpio->pin);
				CLR_PORT_B_PINOUT(gpio->pin);
				SET_PORT_B_PINOUT(gpio->pin);
			}
			else if (mode == output)
			{
				SET_PORT_B_OUTMODE(gpio->pin);
				CLR_PORT_B_PINOUT(gpio->pin);
			}
			break;
		case port_c:
			if (mode == highZ)
			{
				CLR_PORT_C_OUTMODE(gpio->pin);
				CLR_PORT_C_PINOUT(gpio->pin);
			}
			else if (mode == input)
			{
				// enter tri-state high-z as an intermediate, see atmega328p gpio data sheet for more information
				CLR_PORT_C_OUTMODE(gpio->pin);
				CLR_PORT_C_PINOUT(gpio->pin);
				SET_PORT_C_PINOUT(gpio->pin);
			}
			else if (mode == output)
			{
				SET_PORT_C_OUTMODE(gpio->pin);
				CLR_PORT_C_PINOUT(gpio->pin);
			}
			break;
		case port_d:
			if (mode == highZ)
			{
				CLR_PORT_D_OUTMODE(gpio->pin);
				CLR_PORT_D_PINOUT(gpio->pin);
			}
			else if (mode == input)
			{
				// enter tri-state high-z as an intermediate, see atmega328p gpio data sheet for more information
				CLR_PORT_D_OUTMODE(gpio->pin);
				CLR_PORT_D_PINOUT(gpio->pin);
				SET_PORT_D_PINOUT(gpio->pin);
			}
			else if (mode == output)
			{
				SET_PORT_D_OUTMODE(gpio->pin);
				CLR_PORT_D_PINOUT(gpio->pin);
			}
			break;
	}
}

void gpio_PCINTConfig(gpio_t *gpio)
{
	switch (gpio->port)
	{
		case port_b:
			CONFIGURE_PCINT0_PIN_FOR_INT(gpio->pin);
			break;
		case port_c:
			CONFIGURE_PCINT1_PIN_FOR_INT(gpio->pin);
			break;
		case port_d:
			CONFIGURE_PCINT2_PIN_FOR_INT(gpio->pin);
			break;
	}
}

void gpio_PCINTDeconfig(gpio_t *gpio)
{
	switch (gpio->port)
	{
		case port_b:
			DECONFIGURE_PCINT0_PIN_FOR_INT(gpio->pin);
			break;
		case port_c:
			DECONFIGURE_PCINT1_PIN_FOR_INT(gpio->pin);
			break;
		case port_d:
			DECONFIGURE_PCINT2_PIN_FOR_INT(gpio->pin);
			break;
	}
}

void gpio_PCINTEnableIRQ(gpio_t *gpio)
{
	switch (gpio->port)
	{
		case port_b:
			ENABLE_PCINT0_INT;
			break;
		case port_c:
			ENABLE_PCINT1_INT;
			break;
		case port_d:
			ENABLE_PCINT2_INT;
			break;
	}
}

void gpio_PCINTDisableIRQ(gpio_t *gpio)
{
	switch (gpio->port)
	{
		case port_b:
			DISABLE_PCINT0_INT;
			break;
		case port_c:
			DISABLE_PCINT1_INT;
			break;
		case port_d:
			DISABLE_PCINT2_INT;
			break;
	}
}

void gpio_EXTINTConfig(ext_int_t intPin, ext_int_mode_t mode)
{
	uint8_t bits = 0;

	switch (mode)
	{
		case int_req_logic_low:
			EXT_INT_SRC_LOGIC_LOW(bits);
			break;
		case int_req_logic_any:
			EXT_INT_SRC_LOGIC_ANY(bits);
			break;
		case int_req_falling_edge:
			EXT_INT_SRC_FALLING(bits);
			break;
		case int_req_rising_edge:
			EXT_INT_SRC_RISING(bits);
			break;
	}

	if (intPin == ext_int_0)
	{
		bits &= EXT_INT_0_MODE_BITMASK;
		EICRA &= ~(EXT_INT_0_MODE_BITMASK);
		EICRA |= bits;
	}
	else if (intPin == ext_int_1)
	{
		bits <<= 2;
		bits &= EXT_INT_1_MODE_BITMASK;
		EICRA &= ~(EXT_INT_1_MODE_BITMASK);
		EICRA |= bits;
	}
}

void gpio_EXTINTDeconfig(ext_int_t intPin)
{
	if (intPin == ext_int_0)
		EICRA &= ~(EXT_INT_0_MODE_BITMASK);
	else if (intPin == ext_int_1)
		EICRA &= ~(EXT_INT_1_MODE_BITMASK);
}

void gpio_EXTINTEnableIRQ(ext_int_t intPin)
{
	if (intPin == ext_int_0)
		ENABLE_EXT_INT_PIN_0;
	else if (intPin == ext_int_1)
		ENABLE_EXT_INT_PIN_1;
}

void gpio_EXTINTDisableIRQ(ext_int_t intPin)
{
	if (intPin == ext_int_0)
		DISABLE_EXT_INT_PIN_0;
	else if (intPin == ext_int_1)
		DISABLE_EXT_INT_PIN_1;
}

void gpio_outputLogicHigh(gpio_t *gpio)
{
	switch (gpio->port)
	{
		case port_b:
			SET_PORT_B_PINOUT(gpio->pin);
			break;
		case port_c:
			SET_PORT_C_PINOUT(gpio->pin);
			break;
		case port_d:
			SET_PORT_D_PINOUT(gpio->pin);
			break;
	}
}

void gpio_outputLogicLow(gpio_t *gpio)
{
	switch (gpio->port)
	{
		case port_b:
			CLR_PORT_B_PINOUT(gpio->pin);
			break;
		case port_c:
			CLR_PORT_C_PINOUT(gpio->pin);
			break;
		case port_d:
			CLR_PORT_D_PINOUT(gpio->pin);
			break;
	}
}

void gpio_outputLogic(gpio_t *gpio, bool_t setLogicHigh)
{
	if (setLogicHigh)
		gpio_outputLogicHigh(gpio);
	else
		gpio_outputLogicLow(gpio);
}

uint8_t gpio_readInputLogicLevel(gpio_t *gpio)
{
	uint8_t pinVal = 0;

	NOP;
	switch (gpio->port)
	{
		case port_b:
			READ_PORT_B_INPUT(pinVal, gpio->pin);
			break;
		case port_c:
			READ_PORT_C_INPUT(pinVal, gpio->pin);
			break;
		case port_d:
			READ_PORT_D_INPUT(pinVal, gpio->pin);
			break;
	}

	if (pinVal > 0)
		pinVal = 1;

	return pinVal;
}
