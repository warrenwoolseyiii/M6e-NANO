/* ========================================================================== *
 *               Copyright (C) Warren Woolsey - All Rights Reserved           *
 *  Unauthorized copying of this file, via any medium is strictly prohibited  *
 *                       Proprietary and confidential.                        *
 * Written by Warren Woolsey                                                  *
 * ========================================================================== */
#include <osDepStub.h>
#include <tm_reader.h>
#include <hal/atmega328p/core/core.h>

void sampleRead();
bool_t checkError(TMR_Status status);

int main(void)
{
	initOsDepStub();
	sampleRead();
	while (1)
	{
		
	}
}

bool_t checkError(TMR_Status status)
{
	if (TMR_SUCCESS != status)
		return FALSE;

	return TRUE;
}

void sampleRead()
{
	TMR_Reader r, *rp;
	TMR_Status ret;
	TMR_Region region;

	rp = &r;
	ret = TMR_create(rp, "eapi:///COM2");
	if (!checkError(ret))
		TMR_destroy(rp);

}