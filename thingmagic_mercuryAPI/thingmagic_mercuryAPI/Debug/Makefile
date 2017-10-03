################################################################################
# Automatically-generated file. Do not edit!
################################################################################

SHELL := cmd.exe
RM := rm -rf

USER_OBJS :=

LIBS := 
PROJ := 

O_SRCS := 
C_SRCS := 
S_SRCS := 
S_UPPER_SRCS := 
OBJ_SRCS := 
ASM_SRCS := 
PREPROCESSING_SRCS := 
OBJS := 
OBJS_AS_ARGS := 
C_DEPS := 
C_DEPS_AS_ARGS := 
EXECUTABLES := 
OUTPUT_FILE_PATH :=
OUTPUT_FILE_PATH_AS_ARGS :=
AVR_APP_PATH :=$$$AVR_APP_PATH$$$
QUOTE := "
ADDITIONAL_DEPENDENCIES:=
OUTPUT_FILE_DEP:=
LIB_DEP:=
LINKER_SCRIPT_DEP:=

# Every subdirectory with source files must be described here
SUBDIRS := 


# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS +=  \
../hex_bytes.c \
../osDepStub.c \
../osdep_dummy.c \
../serialTransportStub.c \
../serial_reader.c \
../serial_reader_l3.c \
../serial_transport_dummy.c \
../tmr_param.c \
../tmr_strerror.c \
../tmr_utils.c \
../tm_reader.c


PREPROCESSING_SRCS += 


ASM_SRCS += 


OBJS +=  \
hex_bytes.o \
osDepStub.o \
osdep_dummy.o \
serialTransportStub.o \
serial_reader.o \
serial_reader_l3.o \
serial_transport_dummy.o \
tmr_param.o \
tmr_strerror.o \
tmr_utils.o \
tm_reader.o

OBJS_AS_ARGS +=  \
hex_bytes.o \
osDepStub.o \
osdep_dummy.o \
serialTransportStub.o \
serial_reader.o \
serial_reader_l3.o \
serial_transport_dummy.o \
tmr_param.o \
tmr_strerror.o \
tmr_utils.o \
tm_reader.o

C_DEPS +=  \
hex_bytes.d \
osDepStub.d \
osdep_dummy.d \
serialTransportStub.d \
serial_reader.d \
serial_reader_l3.d \
serial_transport_dummy.d \
tmr_param.d \
tmr_strerror.d \
tmr_utils.d \
tm_reader.d

C_DEPS_AS_ARGS +=  \
hex_bytes.d \
osDepStub.d \
osdep_dummy.d \
serialTransportStub.d \
serial_reader.d \
serial_reader_l3.d \
serial_transport_dummy.d \
tmr_param.d \
tmr_strerror.d \
tmr_utils.d \
tm_reader.d

OUTPUT_FILE_PATH +=libthingmagic_mercuryAPI.a

OUTPUT_FILE_PATH_AS_ARGS +=libthingmagic_mercuryAPI.a

ADDITIONAL_DEPENDENCIES:=

OUTPUT_FILE_DEP:= ./makedep.mk

LIB_DEP+=  \
C:/Users/warre/Repositories/RFID/ATMEGA328p_HAL/HAL_Dev/Debug/libHAL_Dev.a

LINKER_SCRIPT_DEP+= 


# AVR32/GNU C Compiler























./%.o: .././%.c
	@echo Building file: $<
	@echo Invoking: AVR8/GNU C Compiler : 5.4.0
	$(QUOTE)C:\Program Files (x86)\Atmel\Studio\7.0\toolchain\avr8\avr8-gnu-toolchain\bin\avr-gcc.exe$(QUOTE)  -x c -funsigned-char -funsigned-bitfields -DDEBUG -DTMR_ENABLE_SERIAL_READER_ONLY=1  -UTMR_ENABLE_ERROR_STRINGS -UTMR_ENABLE_PARAM_STRINGS  -I"C:\Program Files (x86)\Atmel\Studio\7.0\Packs\atmel\ATmega_DFP\1.2.132\include" -I"../../../../ATMEGA328p_HAL/HAL_Dev"  -O1 -ffunction-sections -fdata-sections -fpack-struct -fshort-enums -g2 -Wall -mmcu=atmega328p -B "C:\Program Files (x86)\Atmel\Studio\7.0\Packs\atmel\ATmega_DFP\1.2.132\gcc\dev\atmega328p" -c -std=gnu99 -MD -MP -MF "$(@:%.o=%.d)" -MT"$(@:%.o=%.d)" -MT"$(@:%.o=%.o)"   -o "$@" "$<" 
	@echo Finished building: $<
	



# AVR32/GNU Preprocessing Assembler



# AVR32/GNU Assembler




ifneq ($(MAKECMDGOALS),clean)
ifneq ($(strip $(C_DEPS)),)
-include $(C_DEPS)
endif
endif

# Add inputs and outputs from these tool invocations to the build variables 

# All Target
all: $(OUTPUT_FILE_PATH) $(ADDITIONAL_DEPENDENCIES)


$(OUTPUT_FILE_PATH): $(OBJS) $(USER_OBJS) $(OUTPUT_FILE_DEP)
	@echo Building target: $@
	@echo Invoking: AVR8/GNU Archiver : 5.4.0
	$(QUOTE)C:\Program Files (x86)\Atmel\Studio\7.0\toolchain\avr8\avr8-gnu-toolchain\bin\avr-ar.exe$(QUOTE) -r  -o$(OUTPUT_FILE_PATH_AS_ARGS) $(OBJS_AS_ARGS) $(USER_OBJS) $(LIBS) 
	@echo Finished building target: $@
	
	




# Other Targets
clean:
	-$(RM) $(OBJS_AS_ARGS) $(EXECUTABLES)  
	-$(RM) $(C_DEPS_AS_ARGS)   
	rm -rf "libthingmagic_mercuryAPI.elf" "libthingmagic_mercuryAPI.a" "libthingmagic_mercuryAPI.hex" "libthingmagic_mercuryAPI.lss" "libthingmagic_mercuryAPI.eep" "libthingmagic_mercuryAPI.map" "libthingmagic_mercuryAPI.srec" "libthingmagic_mercuryAPI.usersignatures"
	