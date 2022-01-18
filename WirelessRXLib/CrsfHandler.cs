using System;
using System.Collections.Generic;

namespace WirelessRXLib
{
	public class CrsfHandler
	{
		const int CRSF_SUBSET_RC_CHANNELS_PACKED_RESOLUTION = 11; // 11 bits per channel
		const uint CRSF_SUBSET_RC_CHANNELS_PACKED_MASK = 0b11111111111; // 11 bits, get it?!
		private Dictionary<int, Action<byte[]>> handlers = new Dictionary<int, Action<byte[]>>();
		private Action<Message> channelsEvent;
		private CrsfSender sender;
        public CrsfHandler(Action<Message> channelsEvent, CrsfSender sender)
        {
            //this.sensors = sensors;
            this.channelsEvent = channelsEvent;
            this.sender = sender;
            handlers.Add(0x16,HandleChannels);
            handlers.Add(0x14,LinkStats);
			handlers.Add(0x08,HandleBattery);
            //Set these up
            /*
            handlers.Add(0x40, HandleChannels);
            handlers.Add(0x80, HandleSensorDiscover);
            handlers.Add(0x90, HandleSensorDescribe);
            handlers.Add(0xA0, HandleSensorData);
            handlers.Add(0xF0, HandleReceiverBootup);
            */
        }
        public void HandleMessage(byte[] payload)
		{
			int type = payload[2];
			if (handlers.ContainsKey(type))
			{
				handlers[type](payload);
			}
			else
			{
				Console.WriteLine($"RX UNKNOWN {type.ToString()}");//("X2")}");
			}
		}
		public void HandleChannels(byte[] payload)
		{
			Message m = new Message();
			const uint numOfChannels = 16;
			uint readByte = 0;
			int byteIndex = 3;
			int bitsMerged = 0;
			uint readValue = 0;
			for (uint i = 0; i < numOfChannels; i++)
			{
				while (bitsMerged < CRSF_SUBSET_RC_CHANNELS_PACKED_RESOLUTION)
				{
					readByte = payload[byteIndex++];
					readValue |= readByte << bitsMerged;
					bitsMerged += 8;
				}
				m.channelsRaw[i] = (ushort)(readValue & CRSF_SUBSET_RC_CHANNELS_PACKED_MASK);
				//to het 1000 <=> 2000
				//m.channels[i] = ((m.channelsRaw[i]-992)*5/8+1500);
				//to get -1 <=> 1
				m.channels[i] = TICKS_TO_US(m.channelsRaw[i]);
				readValue >>= CRSF_SUBSET_RC_CHANNELS_PACKED_RESOLUTION;
				bitsMerged -= CRSF_SUBSET_RC_CHANNELS_PACKED_RESOLUTION;
			}
			if (channelsEvent != null)
			{channelsEvent(m);}
		}
		public void LinkStats(byte[] payload)
		{}
		public void HandleBattery(byte[] payload)
		{
			const int LEN = 8;
			byte[] p = new byte[LEN+4];
			p[0] = 0xC8;
			p[1] = LEN+2;
			p[2] = 0x08;
			p[3] = payload[3];//voltage high byte
			p[4] = payload[4];//voltage low byte
			p[10] = payload[10];
			p[LEN+3] = CalcCRC(p);

			sender.Send(p,p.Length);
		}
		private byte CalcCRC(byte[] payload)
        {
            int length = payload[1];
            int crc = 0;
            for (int i = 2; i <= length; i++)
            {
                crc = crc8tab[crc ^ payload[i]];
            }
            return (byte)crc;
        }
		byte[] crc8tab = new byte[] {
			0x00,0xBA,0xCE,0x74,0x26,0x9C,0xE8,0x52,0x4C,0xF6,0x82,0x38,0x6A,0xD0,0xA4,0x1E,
			0x98,0x22,0x56,0xEC,0xBE,0x04,0x70,0xCA,0xD4,0x6E,0x1A,0xA0,0xF2,0x48,0x3C,0x86,
			0x8A,0x30,0x44,0xFE,0xAC,0x16,0x62,0xD8,0xC6,0x7C,0x08,0xB2,0xE0,0x5A,0x2E,0x94,
			0x12,0xA8,0xDC,0x66,0x34,0x8E,0xFA,0x40,0x5E,0xE4,0x90,0x2A,0x78,0xC2,0xB6,0x0C,
			0xAE,0x14,0x60,0xDA,0x88,0x32,0x46,0xFC,0xE2,0x58,0x2C,0x96,0xC4,0x7E,0x0A,0xB0,
			0x36,0x8C,0xF8,0x42,0x10,0xAA,0xDE,0x64,0x7A,0xC0,0xB4,0x0E,0x5C,0xE6,0x92,0x28,
			0x24,0x9E,0xEA,0x50,0x02,0xB8,0xCC,0x76,0x68,0xD2,0xA6,0x1C,0x4E,0xF4,0x80,0x3A,
			0xBC,0x06,0x72,0xC8,0x9A,0x20,0x54,0xEE,0xF0,0x4A,0x3E,0x84,0xD6,0x6C,0x18,0xA2,
			0xE6,0x5C,0x28,0x92,0xC0,0x7A,0x0E,0xB4,0xAA,0x10,0x64,0xDE,0x8C,0x36,0x42,0xF8,
			0x7E,0xC4,0xB0,0x0A,0x58,0xE2,0x96,0x2C,0x32,0x88,0xFC,0x46,0x14,0xAE,0xDA,0x60,
			0x6C,0xD6,0xA2,0x18,0x4A,0xF0,0x84,0x3E,0x20,0x9A,0xEE,0x54,0x06,0xBC,0xC8,0x72,
			0xF4,0x4E,0x3A,0x80,0xD2,0x68,0x1C,0xA6,0xB8,0x02,0x76,0xCC,0x9E,0x24,0x50,0xEA,
			0x48,0xF2,0x86,0x3C,0x6E,0xD4,0xA0,0x1A,0x04,0xBE,0xCA,0x70,0x22,0x98,0xEC,0x56,
			0xD0,0x6A,0x1E,0xA4,0xF6,0x4C,0x38,0x82,0x9C,0x26,0x52,0xE8,0xBA,0x00,0x74,0xCE,
			0xC2,0x78,0x0C,0xB6,0xE4,0x5E,0x2A,0x90,0x8E,0x34,0x40,0xFA,0xA8,0x12,0x66,0xDC,
			0x5A,0xE0,0x94,0x2E,0x7C,0xC6,0xB2,0x08,0x16,0xAC,0xD8,0x62,0x30,0x8A,0xFE,0x44} ;

		private float TICKS_TO_US(ushort x){return (float)(x-992)*5/8/500;}

	}
}
		/*
		public void HandleChannels(int sensorID, byte[] data)
		{
			Message m = new Message();
			for (int i = 0; i < 14; i++)
			{
				//Little endian
				byte upperByte = data[2 + (i * 2)];
				byte lowerByte = data[3 + (i * 2)];
				int channelValue = (lowerByte & 0xF) << 8;
				channelValue |= upperByte;
				m.channelsRaw[i] = (ushort)channelValue;
				m.channels[i] = (m.channelsRaw[i] - 1500) / 500f;
			}
			for (int i = 0; i < 4; i++)
			{
				//CH15-19 are spread out 4 bit per original channel
				byte upperByte = data[3 + (i * 6)];
				byte middleByte = data[5 + (i * 6)];
				byte lowerByte = data[7 + (i * 6)];
				int channelValue = (upperByte & 0xF0) >> 4;
				channelValue |= middleByte & 0xF0;
				channelValue = (lowerByte & 0xF0) << 4;
				m.channelsRaw[i + 14] = (ushort)channelValue;
				m.channels[i + 14] = (m.channelsRaw[i] - 1500) / 500f;
			}
			if (channelsEvent != null)
			{
				channelsEvent(m);
			}
		}

		public void HandleSensorDiscover(int sensorID, byte[] data)
		{
			//We don't have this sensor connected, ignore.
			if (sensors[sensorID] == null)
			{
				return;
			}
			//Defend against the infinite loop of echo'ing the packets we have written
			if (ignoreSensor[sensorID])
			{
				ignoreSensor[sensorID] = false;
				return;
			}
			ignoreSensor[sensorID] = true;
			sender.SendDiscover(sensorID);
		}

		public void HandleSensorDescribe(int sensorID, byte[] data)
		{
			//We don't have this sensor connected, ignore.
			if (sensors[sensorID] == null)
			{
				return;
			}
			//Ignore our own messages, request is only 4 bytes.
			if (data[0] != 4)
			{
				return;
			}
			sender.SendSensorDescribe(sensorID, sensors[sensorID]);
		}

		public void HandleSensorData(int sensorID, byte[] data)
		{
			//We don't have this sensor connected, ignore.
			if (sensors[sensorID] == null)
			{
				return;
			}
			//Ignore our own messages, request is only 4 bytes.
			if (data[0] != 4)
			{
				return;
			}
			sender.SendSensorData(sensorID, sensors[sensorID]);
		}

		public void HandleReceiverBootup(int sensorID, byte[] data)
		{
			//F0 may be some sensor bootup message for firmware updating?
		}
		*/
	//}
//}