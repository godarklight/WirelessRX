/*
Message frame is:

*/


using System;
using System.Collections.Generic;


namespace WirelessRXLib
{
    public class CRSFDecoder : IDecoder
    {

        const int CRSF_PAYLOAD_SIZE_MAX = 62;
		const int CRSF_SUBSET_RC_CHANNELS_PACKED_RESOLUTION = 11; // 11 bits per channel
		const uint CRSF_SUBSET_RC_CHANNELS_PACKED_MASK = 0b11111111111; // 11 bits, get it?!

		// AddressTypes
		const byte CRSF_ADDRESS_FLIGHT_CONTROLLER = 0xC8; // 200
		// FrameTypes
		const byte CRSF_FRAMETYPE_LINK_STATISTICS = 0x14;
		const byte CRSF_FRAMETYPE_RC_CHANNELS_PACKED = 0x16;

		private Crc8 crcCalc = new Crc8(0xD5);

        private CRSFHandler handler;

        public CRSFDecoder(CRSFHandler handler)
        {
            this.handler = handler;
        }
        public void Decode(byte[] bytes, int length)
        {
            while (length >=CRSF_PAYLOAD_SIZE_MAX+2)
            {
                byte payloadLength = bytes[1];
                int FrameLenth = 2;
                if(payloadLength <2||payloadLength>CRSF_PAYLOAD_SIZE_MAX)
                {
                    //Keep shifting until its 61 or 0
                    Array.Copy(bytes,1,bytes,0,bytes.Length-1);
                }
                FrameLenth = payloadLength+2;
                crcCalc.Out = 0;
                for(int payloadIdx=0; payloadIdx<payloadLength-1;++payloadIdx)
                    {crcCalc.Add(bytes[payloadIdx+2]);}
                byte inCrc = bytes[2+payloadLength-1];
                if(crcCalc.Out != inCrc)
                {
                    
                }
                else
                {
                    if((bytes[0]==CRSF_ADDRESS_FLIGHT_CONTROLLER)
                        && (bytes[2]==CRSF_FRAMETYPE_RC_CHANNELS_PACKED))
                    {
                        //const uint numOfChannels = 16;
			    		//uint readByte = 0;
			    		//int byteIndex = 3;
		    			//int bitsMerged = 0;
		    			//uint readValue = 0;
                        //for (uint n = 0; n < numOfChannels; n++)
					    //{
						//    while (bitsMerged < CRSF_SUBSET_RC_CHANNELS_PACKED_RESOLUTION)
						//    {
						//    	readByte = bytes[byteIndex++];
						//    	readValue |= readByte << bitsMerged;
						//    	bitsMerged += 8;
						//    }
                            handler.HandleMessage(bytes);
    					//	channelData[n] = (int)map((readValue & CRSF_SUBSET_RC_CHANNELS_PACKED_MASK), 191, 1792, 1000, 2000);
		    			//	readValue >>= CRSF_SUBSET_RC_CHANNELS_PACKED_RESOLUTION;
			    		//	bitsMerged -= CRSF_SUBSET_RC_CHANNELS_PACKED_RESOLUTION;
				    	//}
       					//retVal = (int)numOfChannels;

                    }
                }
            for(int index = 0; index<bytes.Length;index++)
                {bytes[index] = 0;}
            }
        }
    }
}