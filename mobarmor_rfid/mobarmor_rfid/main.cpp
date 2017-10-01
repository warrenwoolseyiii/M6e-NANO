/* ========================================================================== *
 *               Copyright (C) Warren Woolsey - All Rights Reserved           *
 *  Unauthorized copying of this file, via any medium is strictly prohibited  *
 *                       Proprietary and confidential.                        *
 * Written by Warren Woolsey                                                  *
 * ========================================================================== */
#include "mercuryAPI/osDepStub.h"
#include "mercuryAPI/serialTransportStub.h"

int main(void)
{
	if (!initOsDepStub())
		return 0;

	if (!initSerialTransport(baud_115200))
		return 0;

	while (1)
	{
		
	}
}