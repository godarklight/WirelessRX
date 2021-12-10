/*
Message frame is:
Length (1 byte) | MessageType(0xF0) & SensorID(0x0F) (1 byte) | Data(variable length) | Checksum(2)
The length field includes the length byte and the checksum bytes
Checksum is computed 0xFFFF - each byte before the checksum.
*/


using System;
using System.Collections.Generic;

namespace WirelessRXLib
{
    public class SbusDecoder
    {
        private bool syncronised = false;
        private byte[] processMessage = new byte[64];
        private int processMessagePos = 24;
        private SbusHandler handler;

        public SbusDecoder(SbusHandler handler)
        {
            this.handler = handler;
        }

        public void Decode(byte[] bytes, int length)
        {
            int incomingReadLeft = length;
            int incomingReadPos = 0;

            while (incomingReadLeft > 0)
            {
                //Syncronise the stream by finding a 0x2040 header
                while (!syncronised && incomingReadLeft > 0)
                {
                    processMessage[processMessagePos] = bytes[incomingReadPos];
                    incomingReadPos++;
                    incomingReadLeft--;
                    //Footer
                    if (processMessagePos == 24 && processMessage[24] == 0x00)
                    {
                        processMessagePos = 0;
                        continue;
                    }
                    //Header
                    if (processMessagePos == 0 && processMessage[0] == 0x0F)
                    {
                        processMessagePos = 1;
                        syncronised = true;
                    }
                    else
                    {
                        processMessagePos = 24;
                    }
                }

                //We can't continue unless syncronised
                if (!syncronised)
                {
                    return;
                }

                //Read message
                if (incomingReadLeft > 0)
                {
                    int bytesToRead = 25 - processMessagePos;
                    if (bytesToRead > incomingReadLeft)
                    {
                        bytesToRead = incomingReadLeft;
                    }
                    Array.Copy(bytes, incomingReadPos, processMessage, processMessagePos, bytesToRead);
                    processMessagePos += bytesToRead;
                    incomingReadPos += bytesToRead;
                    incomingReadLeft -= bytesToRead;
                }

                //Message not yet fully received, wait.
                if (processMessagePos != 25)
                {
                    return;
                }

                if (processMessage[0] != 0x0F || processMessage[24] != 0x00)
                {
                    syncronised = false;
                }
                else
                {
                    handler.HandleMessage(processMessage);
                }
                processMessagePos = 0;
            }
        }
    }
}