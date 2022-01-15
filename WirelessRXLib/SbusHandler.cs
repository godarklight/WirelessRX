using System;
using System.Collections.Generic;

namespace WirelessRXLib
{
<<<<<<< Updated upstream
	public class SbusHandler
	{
		private Dictionary<int, Action<int, byte[]>> handlers = new Dictionary<int, Action<int, byte[]>>();
		private Action<Message> channelsEvent;
		private Sender sender;
		private bool[] ignoreSensor = new bool[16];

		public SbusHandler(Action<Message> channelsEvent, Sender sender)
		{
			this.channelsEvent = channelsEvent;
			this.sender = sender;
		}
=======
    public class SbusHandler
    {
        private Dictionary<int, Action<int, byte[]>> handlers = new Dictionary<int, Action<int, byte[]>>();
        private Action<Message> channelsEvent;
        private IbusSender sender;
        private bool[] ignoreSensor = new bool[16];

        public SbusHandler(Action<Message> channelsEvent, IbusSender sender)
        {
            this.channelsEvent = channelsEvent;
            this.sender = sender;
        }
>>>>>>> Stashed changes

		public void HandleMessage(byte[] message)
		{
			Message m = new Message();
			for (int i = 0; i < 16; i++)
			{
				int channelValue = 0;
				int startPos = i * 11;
				int startByte = startPos / 8;
				int startLhs = startPos % 8;
				for (int j = 0; j < 3; j++)
				{
					channelValue |= message[1 + startByte + j] << (8 * j);
				}
				channelValue >>= startLhs;
				channelValue &= 0x7FF;
				/*
				if (i == 0)
				{
					Console.WriteLine(channelValue);
				}
				*/
				//Sbus is 0-2048 being -150% to 150%.
				//My TX seems to center at 990 and range is about 800. Uncomment above block to get channel 1 decimal value.
				m.channels[i] = (channelValue - 990) / 800f;
				m.channelsRaw[i] = (ushort)((channelValue * 1500) / 2048);
			}
			//Digital channels
			bool channel17 = (message[23] & 1) > 0;
			bool channel18 = (message[23] & 2) > 0;
			if (channel17)
			{
				m.channelsRaw[16] = 2048;
				m.channels[16] = 1f;
			}
			else
			{
				m.channelsRaw[16] = 1024;
				m.channels[16] = -1f;
			}
			if (channel18)
			{
				m.channelsRaw[17] = 2048;
				m.channels[17] = 1f;
			}
			else
			{
				m.channelsRaw[17] = 1024;
				m.channels[17] = -1f;
			}
			m.framelost = (message[23] & 4) > 0;
			m.failsafe = (message[23] & 8) > 0;
			if (channelsEvent != null)
			{
				channelsEvent(m);
			}
		}
	}
}