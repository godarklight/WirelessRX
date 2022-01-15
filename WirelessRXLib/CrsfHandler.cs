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

<<<<<<< Updated upstream
		private Sender sender;
		public CrsfHandler(Action<Message> channelsEvent, Sender sender)
		{
			//this.sensors = sensors;
			this.channelsEvent = channelsEvent;
			this.sender = sender;
			handlers.Add(0x16,HandleChannels);
			handlers.Add(0x02,HandleGPS);
			//Set these up
			/*
			handlers.Add(0x40, HandleChannels);
			handlers.Add(0x80, HandleSensorDiscover);
			handlers.Add(0x90, HandleSensorDescribe);
			handlers.Add(0xA0, HandleSensorData);
			handlers.Add(0xF0, HandleReceiverBootup);
			*/
		}
=======
        private CrsfSender sender;
        public CrsfHandler(Action<Message> channelsEvent, CrsfSender sender)
        {
            //this.sensors = sensors;
            this.channelsEvent = channelsEvent;
            this.sender = sender;
            handlers.Add(0x16,HandleChannels);
            handlers.Add(0x02,HandleGPS);
            //Set these up
            /*
            handlers.Add(0x40, HandleChannels);
            handlers.Add(0x80, HandleSensorDiscover);
            handlers.Add(0x90, HandleSensorDescribe);
            handlers.Add(0xA0, HandleSensorData);
            handlers.Add(0xF0, HandleReceiverBootup);
            */
        }
>>>>>>> Stashed changes

		public void HandleMessage(byte[] payload)
		{
			int type = payload[2];
			if (handlers.ContainsKey(type))
			{
				handlers[type](payload);
			}
			else
			{
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
		public void HandleGPS(byte[] payload)
			{}
			private float TICKS_TO_US1500(ushort x){return (float)(x-992)*5/8+1500;}
			private float TICKS_TO_US(ushort x){return (float)(x-992)*5/8/500;}
			private float US_TO_TICKS1500(ushort x){return (float)(x-1500)*5/8+992;}
			private float US_TO_TICKS(ushort x){return (float)(x-1500)*5/8/330;}
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