#ifndef __FRC_NET_STRUCTURES_H
#define __FRC_NET_STRUCTURES_H

#include <stdint.h>

// To Robot
#include <NetworkCommunication/FRCComm.h>

// To DS
struct FRCRobotControl {
	union {
		uint8_t control;
		struct {
			uint8_t checkVersions : 1;
			uint8_t test : 1;
			uint8_t resync : 1;
			uint8_t fmsAttached : 1;
			uint8_t autonomous:1;
			uint8_t enabled : 1;
			uint8_t notEStop :1;
			uint8_t reset :1;
		};
	};
	uint8_t batteryVolts;
	uint8_t batteryMilliVolts;
	uint8_t digitalOutputs;
	uint8_t unknown1[4];
	uint16_t teamID;		// This is backwards endian-wise
	uint8_t macAddress[6];
	union {
		uint8_t version[8];
		struct {
			uint8_t year[2];
			uint8_t day[2];
			uint8_t month[2];
			uint8_t revision[2];
		};
	};
	uint8_t unknown2[6];
	uint16_t packetIndex;		// This is backwards endian-wise
};

#endif