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
        WirelessRXFBW wirelessRXFBW;

        public void Start()
        {
            wirelessRXFBW = new WirelessRXFBW();
            io = new SerialIO(FindSerialPort());
            sender = new Sender(io);
            handler = new SbusHandler(wirelessRXFBW.SetChannels, sender);
            decoder = new SbusDecoder(handler);            
            readThread = new Thread(new ThreadStart(ReadLoop));
            readThread.Start();
            DontDestroyOnLoad(this);
            GameEvents.Vehicles.OnVehicleSpawned.AddListener(VehicleSpawned);
        }

        public void OnDestroy()
        {
            running = false;
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

        private void VehicleSpawned(Vehicle v)
        {
            if (v.IsLocalPlayerVehicle)
            {
                v.Autotrim.host.RegisterFBWModule(wirelessRXFBW);
            }
        }
    }
}
