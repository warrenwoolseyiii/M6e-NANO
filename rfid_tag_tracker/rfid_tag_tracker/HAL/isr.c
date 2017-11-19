/*
 * isr.c
 *
 * Created: 8/1/2017 4:26:13 PM
 *  Author: warre
 */ 
#include "isr.h"
#include <avr/interrupt.h>

// local constants
#define NUM_INT_VECTORS 24

// file specific globals
static void (*isrHandlerArray[NUM_INT_VECTORS])(void);
static bool_t gHandlerTableIsInit = FALSE;

// locally defined interrupt vector table
ISR(INT0_vect)
{
	if (isrHandlerArray[INT0_vect_num] != NULL)
		isrHandlerArray[INT0_vect_num]();
}

ISR(INT1_vect)
{
	if (isrHandlerArray[INT1_vect_num] != NULL)
		isrHandlerArray[INT1_vect_num]();
}

ISR(PCINT0_vect)
{
	if (isrHandlerArray[PCINT0_vect_num] != NULL)
		isrHandlerArray[PCINT0_vect_num]();
}

ISR(PCINT1_vect)
{
	if (isrHandlerArray[PCINT1_vect_num] != NULL)
		isrHandlerArray[PCINT1_vect_num]();
}

ISR(PCINT2_vect)
{
	if (isrHandlerArray[PCINT2_vect_num] != NULL)
		isrHandlerArray[PCINT2_vect_num]();
}

ISR(WDT_vect)
{
	if (isrHandlerArray[WDT_vect_num] != NULL)
		isrHandlerArray[WDT_vect_num]();
}

ISR(TIMER2_COMPA_vect)
{
	if (isrHandlerArray[TIMER2_COMPA_vect_num] != NULL)
		isrHandlerArray[TIMER2_COMPA_vect_num]();
}

ISR(TIMER2_COMPB_vect)
{
	if (isrHandlerArray[TIMER2_COMPB_vect_num] != NULL)
		isrHandlerArray[TIMER2_COMPB_vect_num]();
}

ISR(TIMER2_OVF_vect)
{
	if (isrHandlerArray[TIMER2_OVF_vect_num] != NULL)
		isrHandlerArray[TIMER2_OVF_vect_num]();
}

ISR(TIMER1_CAPT_vect)
{
	if (isrHandlerArray[TIMER1_CAPT_vect_num] != NULL)
		isrHandlerArray[TIMER1_CAPT_vect_num]();
}

ISR(TIMER1_COMPA_vect)
{
	if (isrHandlerArray[TIMER1_COMPA_vect_num] != NULL)
		isrHandlerArray[TIMER1_COMPA_vect_num]();
}

ISR(TIMER1_COMPB_vect)
{
	if (isrHandlerArray[TIMER1_COMPB_vect_num] != NULL)
		isrHandlerArray[TIMER1_COMPB_vect_num]();
}

ISR(TIMER1_OVF_vect)
{
	if (isrHandlerArray[TIMER1_OVF_vect_num] != NULL)
		isrHandlerArray[TIMER1_OVF_vect_num]();
}

ISR(TIMER0_COMPA_vect)
{
	if (isrHandlerArray[TIMER0_COMPA_vect_num] != NULL)
		isrHandlerArray[TIMER0_COMPA_vect_num]();
}

ISR(TIMER0_COMPB_vect)
{
	if (isrHandlerArray[TIMER0_COMPB_vect_num] != NULL)
		isrHandlerArray[TIMER0_COMPB_vect_num]();
}

ISR(TIMER0_OVF_vect)
{
	if (isrHandlerArray[TIMER0_OVF_vect_num] != NULL)
		isrHandlerArray[TIMER0_OVF_vect_num]();
}

ISR(SPI_STC_vect)
{
	if (isrHandlerArray[SPI_STC_vect_num] != NULL)
		isrHandlerArray[SPI_STC_vect_num]();
}

ISR(USART_RX_vect)
{
	if (isrHandlerArray[USART_RX_vect_num] != NULL)
		isrHandlerArray[USART_RX_vect_num]();
}

ISR(USART_UDRE_vect)
{
	if (isrHandlerArray[USART_UDRE_vect_num] != NULL)
		isrHandlerArray[USART_UDRE_vect_num]();
}

ISR(USART_TX_vect)
{
	if (isrHandlerArray[USART_TX_vect_num] != NULL)
		isrHandlerArray[USART_TX_vect_num]();
}

ISR(ADC_vect)
{
	if (isrHandlerArray[ADC_vect_num] != NULL)
		isrHandlerArray[ADC_vect_num]();
}

ISR(EE_READY_vect)
{
	if (isrHandlerArray[EE_READY_vect_num] != NULL)
		isrHandlerArray[EE_READY_vect_num]();
}

ISR(ANALOG_COMP_vect)
{
	if (isrHandlerArray[ANALOG_COMP_vect_num] != NULL)
		isrHandlerArray[ANALOG_COMP_vect_num]();
}

// globally defined functions
bool_t isr_isHandlerTableInit()
{
	return gHandlerTableIsInit;
}

void isr_initHandlerTable()
{
	uint8_t i;
	
	for (i = 0; i < NUM_INT_VECTORS; i++)
		isrHandlerArray[i] = NULL;
		
	gHandlerTableIsInit = TRUE;
}

void isr_registerHandlerFunc(uint8_t vectorNumber, void (*func) ())
{
	if (func == NULL)
		return;
	else if (vectorNumber > NUM_INT_VECTORS)
		return;
		
	isrHandlerArray[vectorNumber] = func;
}

void isr_unRegisterHandlerFunc(uint8_t vectorNumber)
{
	if (vectorNumber > NUM_INT_VECTORS)
		return;
		
	isrHandlerArray[vectorNumber] = NULL;
}