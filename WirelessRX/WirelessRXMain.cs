﻿using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using WirelessRXLib;

namespace WirelessRX
{
    public class WirelessRXMain : MonoBehaviour
    {
        public WirelessRXMain Instance
        {
            private set;
            get;
        }
        bool running = true;
        IOInterface io;
        Sender sender;
        IDecoder decoder;
        Thread readThread;
        long channelExpireTime;
        public Message ChannelData
        {
            private set;
            get;
        }
        public Sensor[] Sensors
        {
            private set;
            get;
        }
        bool[] relativeState = new bool[8];
        bool overrideControls = false;
        bool safeEnable = false;
        ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
        SerialDetector detector;

        public void Start()
        {
            Instance = this;
            detector = new SerialDetector(QueueMessage, SerialEvent);
            DontDestroyOnLoad(this);
        }

        private void QueueMessage(string message)
        {
            messageQueue.Enqueue(message);
        }

        private void SerialEvent(int type, SerialPort sp)
        {
            switch (type)
            {
                case 1:
                    StartIBUS(sp);
                    break;
                case 2:
                    StartSBUS(sp);
                    break;
                case 3:
                    StartCRSF(sp);
                    break;
                default:
                    QueueMessage($"Unknown serial type {type}");
                    break;
            }
        }

        private void StartSBUS(SerialPort sp)
        {
            QueueMessage($"[WirelessRX] Detected SBUS on {sp.PortName}, rate {sp.BaudRate}");
            io = new SerialIO(sp);
            sender = new Sender(io);
            SbusHandler handler = new SbusHandler(SetChannelData, sender);
            decoder = new SbusDecoder(handler);
            readThread = new Thread(new ThreadStart(ReadLoop));
            readThread.Start();
        }

        private void StartIBUS(SerialPort sp)
        {
            QueueMessage($"[WirelessRX] Detected IBUS on {sp.PortName}, rate {sp.BaudRate}");
            io = new SerialIO(sp);
            sender = new Sender(io);
            Sensor[] sensors = new Sensor[16];
            //Sensors can go here
            IbusHandler handler = new IbusHandler(SetChannelData, sensors, sender);
            decoder = new IbusDecoder(handler);
            readThread = new Thread(new ThreadStart(ReadLoop));
            readThread.Start();
        }

        private void StartCRSF(SerialPort sp)
        {
            QueueMessage($"[WirelessRX] Detected CRSF on {sp.PortName}, rate {sp.BaudRate}");
            io = new SerialIO(sp);
            sender = new Sender(io);
            CrsfHandler handler = new CrsfHandler(SetChannelData, sender);
            decoder = new CrsfDecoder(handler);
            readThread = new Thread(new ThreadStart(ReadLoop));
            readThread.Start();
        }

        private void OnDestroy()
        {
            running = false;
            DisableOverride();
        }

        private void Update()
        {
            if (messageQueue.TryDequeue(out string messageString))
            {
                if (!messageString.StartsWith("Checking"))
                {
                    ScreenMessage sm = ScreenMessages.PostScreenMessage(messageString, 5, ScreenMessageStyle.UPPER_CENTER);
                    if (sm != null)
                    {
                        sm.color = BalsaColors.NotSoGoodOrange;
                    }
                }
                Debug.Log(messageString);
            }
            if (safeEnable)
            {
                safeEnable = false;
                EnableOverride();
            }
            CheckTimeout();
            if (!overrideControls)
            {
                return;
            }
            InputSettings.Axis_Roll.axis = ChannelData.channels[0];
            InputSettings.Axis_Pitch.axis = ChannelData.channels[1];
            InputSettings.Axis_Throttle.axis = ChannelData.channels[2];
            InputSettings.Axis_Yaw.axis = ChannelData.channels[3];
            InputSettings.EngineAutoStart.axisAsButton.axis = ChannelData.channels[4];
            InputSettings.ThrottleCutoff.axisAsButton.axis = -ChannelData.channels[4];
            InputSettings.Axis_A.axis = ChannelData.channels[5];
            InputSettings.Axis_B.axis = ChannelData.channels[6];
            InputSettings.Axis_C.axis = ChannelData.channels[7];
            InputSettings.Axis_D.axis = ChannelData.channels[8];
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
                this.ChannelData = channelData;
                safeEnable = true;
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
            relativeState[4] = InputSettings.Axis_A.isRelativeAxis;
            relativeState[5] = InputSettings.Axis_B.isRelativeAxis;
            relativeState[6] = InputSettings.Axis_C.isRelativeAxis;
            relativeState[7] = InputSettings.Axis_D.isRelativeAxis;
            InputSettings.Axis_Roll.isRelativeAxis = true;
            InputSettings.Axis_Pitch.isRelativeAxis = true;
            InputSettings.Axis_Throttle.isRelativeAxis = true;
            InputSettings.Axis_Yaw.isRelativeAxis = true;
            InputSettings.Axis_A.isRelativeAxis = true;
            InputSettings.Axis_B.isRelativeAxis = true;
            InputSettings.Axis_C.isRelativeAxis = true;
            InputSettings.Axis_D.isRelativeAxis = true;
            QueueMessage("[WirelessRX] Override enabled");
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
            InputSettings.Axis_A.isRelativeAxis = relativeState[4];
            InputSettings.Axis_B.isRelativeAxis = relativeState[5];
            InputSettings.Axis_C.isRelativeAxis = relativeState[6];
            InputSettings.Axis_D.isRelativeAxis = relativeState[7];
            QueueMessage("[WirelessRX] Override disabled");
        }

        private void ReadLoop()
        {
            byte[] buffer = new byte[64];
            while (running)
            {
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
