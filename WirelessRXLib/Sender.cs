using System;

namespace WirelessRXLib
{
<<<<<<< Updated upstream
	public class Sender
	{
		byte[] sendBuffer = new byte[32];
		IOInterface io;

		public Sender(IOInterface io)
		{
			this.io = io;
		}
=======
    public class IbusSender
    {
        byte[] sendBuffer = new byte[32];
        IOInterface io;

        public IbusSender(IOInterface io)
        {
            this.io = io;
        }
>>>>>>> Stashed changes

		public void SendDiscover(int sensorID)
		{
			sendBuffer[0] = 4;
			sendBuffer[1] = (byte)(0x80 | sensorID);
			SetSendChecksum(2);
			io.Write(sendBuffer, 4);
		}

<<<<<<< Updated upstream
		public void SendSensorDescribe(int sensorID, Sensor sensor)
		{
			sendBuffer[0] = 6;
			sendBuffer[1] = (byte)(0x90 | sensorID);
			sendBuffer[2] = (byte)sensor.type;
			sendBuffer[3] = (byte)sensor.length;
			SetSendChecksum(4);
			io.Write(sendBuffer, 6);
		}

		public void SendSensorData(int sensorID, Sensor sensor)
		{
			int length = sensor.WriteValue(sensorID, sendBuffer);
			SetSendChecksum(length - 2);
			io.Write(sendBuffer, length);
		}


		private void SetSendChecksum(int positionOfChecksum)
		{
			ushort compute = 0xFFFF;
			for (int i = 0; i < positionOfChecksum; i++)
			{
				compute -= sendBuffer[i];
			}
			BitConverter.GetBytes(compute).CopyTo(sendBuffer, positionOfChecksum);
		}
	}
=======
        public void SendSensorDescribe(int sensorID, IbusSensor sensor)
        {
            sendBuffer[0] = 6;
            sendBuffer[1] = (byte)(0x90 | sensorID);
            sendBuffer[2] = (byte)sensor.type;
            sendBuffer[3] = (byte)sensor.length;
            SetSendChecksum(4);
            io.Write(sendBuffer, 6);
        }

        public void SendSensorData(int sensorID, IbusSensor sensor)
        {
            int length = sensor.WriteValue(sensorID, sendBuffer);
            SetSendChecksum(length - 2);
            io.Write(sendBuffer, length);
        }


        private void SetSendChecksum(int positionOfChecksum)
        {
            ushort compute = 0xFFFF;
            for (int i = 0; i < positionOfChecksum; i++)
            {
                compute -= sendBuffer[i];
            }
            BitConverter.GetBytes(compute).CopyTo(sendBuffer, positionOfChecksum);
        }
    }
    public class CrsfSender
    {
        byte[] sendBuffer = new byte[128];
        IOInterface io;

        public CrsfSender(IOInterface io)
        {
            this.io = io;
        }
    }
>>>>>>> Stashed changes
}