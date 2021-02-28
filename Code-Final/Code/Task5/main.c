#include <avr/io.h>
#include <avr/interrupt.h>
#include "uart.h"
#include "oscilloscope.h"
#include <avr/io.h>


int main(void)
{
	OSCI_Init();
	sei();
	while (1)
	{
		OSCI_MainFunction();
	}
}