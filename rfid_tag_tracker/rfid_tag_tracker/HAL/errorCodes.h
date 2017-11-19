/*
 * errorCodes.h
 *
 * Created: 7/17/2017 6:06:24 PM
 *  Author: warre
 */ 
#ifndef ERRORCODES_H_
#define ERRORCODES_H_

#ifdef __cplusplus
extern "C" {
#endif

#include "types.h"

// uart error codes
typedef enum {
	uart_no_error = 0,
	uart_peripheral_uninitialized = 1,
	uart_frame_error = 2,
	uart_data_overrun = 3,
	uart_parity_error = 4,
	uart_illegal_num_data_bits = 5,
	uart_requested_baud_out_of_range = 6,
	uart_illegal_num_stop_bits = 7,
	uart_attempted_to_dereference_null_ptr = 8,
	uart_attempted_to_call_null_cb = 9,
	uart_transaction_timeout = 10,
   uart_internal_buffer_overflow = 11,
	uart_unkown_error = 12
}uart_error_t;

// eeprom error codes
typedef enum {
	eeprom_no_error = 0,
	eeprom_illegal_addr = 1,
	eeprom_illegal_len = 2,
	eeprom_attempted_to_dereference_null_ptr = 3,
	eeprom_timeout = 4
}eeprom_error_t;

// spi error codes
typedef enum {
	spi_no_error = 0,
	spi_timeout = 1,
	spi_attempted_to_dereference_null_ptr = 2
}spi_error_t;

// timer error codes
typedef enum {
	timer_no_error = 0,
	requested_freq_out_of_range = 1,
	selected_functionality_not_supported_on_timer = 2,
	selected_clk_src_not_supported_on_timer = 3,
	external_clk_freq_not_set = 4
}timer_error_t;

// twi error codes
typedef enum {
	twi_no_error = 0,
	twi_timeout = 1,
	twi_received_nack = 2,
	twi_sent_nack = 3,
	twi_arbitration_lost = 4,
	twi_peripheral_uninitialized = 5,
	twi_attempted_to_dereference_null_ptr = 6,
	twi_unknown_status_code = 7	
}twi_error_t;

#ifdef __cplusplus
}
#endif /* __cplusplus */

#endif /* ERRORCODES_H_ */