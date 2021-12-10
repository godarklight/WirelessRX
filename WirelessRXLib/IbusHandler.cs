using System;
using System.Collections.Generic;

namespace WirelessRXLib
{
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
                m.channelsRaw[i] = BitConverter.ToUInt16(data, 2 + (i * 2));
                m.channels[i] = (m.channelsRaw[i] - 1500) / 500f;
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