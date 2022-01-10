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
		// AddressTypes
		const byte CRSF_ADDRESS_FLIGHT_CONTROLLER = 0xC8; // 200
		// FrameTypes
		const byte CRSF_FRAMETYPE_LINK_STATISTICS = 0x14;
		const byte CRSF_FRAMETYPE_RC_CHANNELS_PACKED = 0x16;

		private Crc8 crcCalc = new Crc8(0xD5);

        private CRSFHandler handler;

        byte[] bytes = new byte[62];
        public CRSFDecoder(CRSFHandler handler)
        {
            this.handler = handler;
        }
        public void Decode(byte[] bytes, int length)
        {
            if(bytes[0]==200)
            {

            int FrameLength = 2;
            byte len = bytes[1];
			if (len < 2 || len > CRSF_PAYLOAD_SIZE_MAX)
			{
                Array.Copy(bytes,1,bytes,0,bytes.Length-1);
            }

                FrameLength = len+2;
                //crcCalc.Out = 0;
                //for(int payloadIdx=0; payloadIdx<len-2;++payloadIdx)
                //    {crcCalc.Add(bytes[payloadIdx]);}
                //byte inCrc = bytes[2+len-1];
                //if(crcCalc.Out != inCrc)
                //{
                   
                //}
                //else
                {
                    if(bytes[0]==CRSF_ADDRESS_FLIGHT_CONTROLLER)
                    {
                        if(bytes[2]==CRSF_FRAMETYPE_RC_CHANNELS_PACKED)
                        {
                        handler.HandleMessage(bytes);
                        }
                    }
                }
                for(int index = 0; index<bytes.Length;index++)
                    {bytes[index] = 0;}
            }
        }
    }
}