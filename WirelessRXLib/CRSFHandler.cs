using System;
using System.Collections.Generic;

namespace WirelessRXLib
{

    public class Crc8
	{
		private byte[] _table = new byte[256];

		public Crc8(byte poly)
		{
			GenerateTable(poly);
			Out = 0;
		}

		public void Add(byte b)
		{
			_out = _table[_out ^ b];
		}

		private byte _out = 0;
		public byte Out
		{
			get { return _out;  }
			set { _out = 0;  }
		}

		private void GenerateTable(byte poly)
		{
			for (uint i = 0; i < 256; ++i)
			{
				uint curr = i;
				for (uint j = 0; j < 8; ++j)
				{
					if ((curr & 0x80) != 0)
					{
						curr = (curr << 1) ^ poly;
					}
					else
					{
						curr <<= 1;
					}
				}

				_table[i] = (byte)curr;
			}
		}
	}
    public class CRSFHandler
    {
		const int CRSF_SUBSET_RC_CHANNELS_PACKED_RESOLUTION = 11; // 11 bits per channel
		const uint CRSF_SUBSET_RC_CHANNELS_PACKED_MASK = 0b11111111111; // 11 bits, get it?!
        private Dictionary<int, Action<int, byte[]>> handlers = new Dictionary<int, Action<int, byte[]>>();
        private Action<Message> channelsEvent;
        private Sender sender;
        public CRSFHandler(Action<Message> channelsEvent,Sender sender)
        {
            this.channelsEvent = channelsEvent;
            this.sender = sender;
        }
          public void HandleMessage(byte[] message)
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
			    	readByte = message[byteIndex++];
			    	readValue |= readByte << bitsMerged;
			    	bitsMerged += 8;
			    }
                m.channels[i] = (int)map((readValue & CRSF_SUBSET_RC_CHANNELS_PACKED_MASK), 191, 1792, 1000, 2000);
				readValue >>= CRSF_SUBSET_RC_CHANNELS_PACKED_RESOLUTION;
				bitsMerged -= CRSF_SUBSET_RC_CHANNELS_PACKED_RESOLUTION;
            }
            if (channelsEvent != null)
            {   channelsEvent(m);}
        }
        static private uint map(uint val, uint in_min, uint in_max, uint out_min, uint out_max)
		{
			// constrain(retVal, out_min, out_max)
			if (val < in_min)
				return out_min;
			if (val > in_max)
				return out_max;
			return (val - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;

		}
    }
}