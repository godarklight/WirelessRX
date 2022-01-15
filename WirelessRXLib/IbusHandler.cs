using System;
using System.Collections.Generic;

namespace WirelessRXLib
{
<<<<<<< Updated upstream
	public class IbusHandler
	{
		private Dictionary<int, Action<int, byte[]>> handlers = new Dictionary<int, Action<int, byte[]>>();
		private Action<Message> channelsEvent;
		private Sensor[] sensors;
		private Sender sender;
		private bool[] ignoreSensor = new bool[16];

		public IbusHandler(Action<Message> channelsEvent, Sensor[] sensors, Sender sender)
		{
			this.sensors = sensors;
			this.channelsEvent = channelsEvent;
			this.sender = sender;
			handlers.Add(0x40, HandleChannels);
			handlers.Add(0x80, HandleSensorDiscover);
			handlers.Add(0x90, HandleSensorDescribe);
			handlers.Add(0xA0, HandleSensorData);
			handlers.Add(0xF0, HandleReceiverBootup);
		}
=======
    public class IbusHandler
    {
        private Dictionary<int, Action<int, byte[]>> handlers = new Dictionary<int, Action<int, byte[]>>();
        private Action<Message> channelsEvent;
        private IbusSensor[] sensors;
        private IbusSender sender;
        private bool[] ignoreSensor = new bool[16];

        public IbusHandler(Action<Message> channelsEvent, IbusSensor[] sensors, IbusSender sender)
        {
            this.sensors = sensors;
            this.channelsEvent = channelsEvent;
            this.sender = sender;
            handlers.Add(0x40, HandleChannels);
            handlers.Add(0x80, HandleSensorDiscover);
            handlers.Add(0x90, HandleSensorDescribe);
            handlers.Add(0xA0, HandleSensorData);
            handlers.Add(0xF0, HandleReceiverBootup);
        }
>>>>>>> Stashed changes

		public void HandleMessage(byte[] message)
		{
			int messageType = message[1] & 0xF0;
			int sensorID = message[1] & 0x0F;
			if (handlers.ContainsKey(messageType))
			{
				handlers[messageType](sensorID, message);
			}
			else
			{
				Console.WriteLine($"RX UNKNOWN {messageType.ToString("X2")} sensor {sensorID}");
			}
		}

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
	}
}