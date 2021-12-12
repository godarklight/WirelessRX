using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using WirelessRXLib;

namespace WirelessRX
{
    public class WirelessRXMain : MonoBehaviour
    {
        bool running = true;
        IOInterface io;
        Sender sender;
        SbusHandler handler;
        SbusDecoder decoder;
        Thread readThread;
        long channelExpireTime;
        Message channelData;
        bool[] relativeState = new bool[4];
        bool overrideControls = false;

        public void Start()
        {
            io = new SerialIO(FindSerialPort());
            sender = new Sender(io);
            handler = new SbusHandler(SetChannelData, sender);
            decoder = new SbusDecoder(handler);
            readThread = new Thread(new ThreadStart(ReadLoop));
            readThread.Start();
            DontDestroyOnLoad(this);
        }

        public void OnDestroy()
        {
            running = false;
            DisableOverride();
        }

        public void Update()
        {
            CheckTimeout();
            if (!overrideControls)
            {
                return;
            }
            InputSettings.Axis_Roll.axis = channelData.channels[0];
            InputSettings.Axis_Pitch.axis = -channelData.channels[1];
            InputSettings.Axis_Throttle.axis = channelData.channels[2];
            InputSettings.Axis_Yaw.axis = channelData.channels[3];
            Debug.Log("Axis Update");
        }

        private void CheckTimeout()
        {
            long currentTime = DateTime.UtcNow.Ticks;
            if (currentTime > channelExpireTime && overrideControls)
            {
                DisableOverride();
            }
        }

        private void SetChannelData(Message channelData)
        {
            if (!channelData.failsafe)
            {
                channelExpireTime = DateTime.UtcNow.Ticks + TimeSpan.TicksPerSecond;
                this.channelData = channelData;
                EnableOverride();
            }
        }

        private void EnableOverride()
        {
            if (overrideControls)
            {
                return;
            }
            overrideControls = true;
            relativeState[0] = InputSettings.Axis_Roll.isRelativeAxis;
            relativeState[1] = InputSettings.Axis_Pitch.isRelativeAxis;
            relativeState[2] = InputSettings.Axis_Throttle.isRelativeAxis;
            relativeState[3] = InputSettings.Axis_Yaw.isRelativeAxis;
            InputSettings.Axis_Roll.isRelativeAxis = true;
            InputSettings.Axis_Pitch.isRelativeAxis = true;
            InputSettings.Axis_Throttle.isRelativeAxis = true;
            InputSettings.Axis_Yaw.isRelativeAxis = true;
            Debug.Log("[WirelessRX] Override enabled");
        }

        private void DisableOverride()
        {
            if (!overrideControls)
            {
                return;
            }
            overrideControls = false;
            InputSettings.Axis_Roll.isRelativeAxis = relativeState[0];
            InputSettings.Axis_Pitch.isRelativeAxis = relativeState[1];
            InputSettings.Axis_Throttle.isRelativeAxis = relativeState[2];
            InputSettings.Axis_Yaw.isRelativeAxis = relativeState[3];
            Debug.Log("[WirelessRX] Override disabled");
        }

        private string FindSerialPort()
        {
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0)
            {
                //Return the last serial port as this is almost certainly the last one plugged in.
                return ports[ports.Length - 1];
            }
            return null;
        }

        private void ReadLoop()
        {
            while (running)
            {
                byte[] buffer = new byte[64];
                if (io.Available() > 0)
                {
                    int bytesToRead = io.Available();
                    if (bytesToRead > buffer.Length)
                    {
                        bytesToRead = buffer.Length;
                    }
                    io.Read(buffer, bytesToRead);
                    decoder.Decode(buffer, bytesToRead);
                }
                else
                {
                    Thread.Sleep(5);
                }
            }
        }
    }
}
