#ifndef MESSAGE_H_
#define MESSAGE_H_

#include <stdlib.h>
#include <string.h>
#include <stdint.h>
#include <stdbool.h>

class MSG {
// variables
public:
	uint8_t *_payload;
private:
	const uint16_t _maxPayloadSize = 64;
	uint16_t _type;
	uint16_t _payloadSize;

// methods
public:
	MSG( uint16_t type = 0, uint16_t size = 0, uint8_t *payload = 0 )
	{
		_type = type;
		
		if (size > _maxPayloadSize) size = _maxPayloadSize;
		_payloadSize = size;

		_payload = (uint8_t *)malloc( (sizeof( uint8_t ) * _payloadSize) );
		if ((_payload != 0) && (payload != 0)) 
			memcpy( (void *)_payload, (void *)payload, _payloadSize );
		else 
		{
			free( _payload );
			_payload = 0;	
		} 
	}

	~MSG()
	{
		free( _payload );
		_payload = 0;
	}

	bool setParams( uint16_t type = 0, uint16_t size = 0, uint8_t *payload = 0 )
	{
		_type = type;
		
		if (size > _maxPayloadSize) return false;
		_payloadSize = size;

		_payload = (uint8_t *)malloc( (sizeof( uint8_t ) * _payloadSize) );
		if ((_payload != 0) && (payload != 0))
		{
			memcpy( (void *)_payload, (void *)payload, _payloadSize );
			return true;
		}
		else
		{
			free( _payload );
			_payload = 0;
			return false;
		}
	}

	uint16_t getType()
	{
		return _type;
	}

	uint16_t getSize()
	{
		return _payloadSize;
	}

	uint16_t getPtrToPayload( uint8_t *ptr )
	{
		if ((_payloadSize != 0) && (_payload != 0)) 
			ptr = _payload;
		else 
			return 0;

		return _payloadSize;
	}

	uint16_t copyPayload( uint16_t maxCpyLen, uint8_t *dst )
	{
		uint16_t tmpLen = _payloadSize;
		if (_payloadSize > maxCpyLen)	tmpLen = maxCpyLen;
		memcpy( (void *)dst, (void *)_payload, tmpLen );
		return tmpLen;
	}
};



#endif /* MESSAGE_H_ */