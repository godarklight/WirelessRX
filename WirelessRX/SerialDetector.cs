using System;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

namespace WirelessRX
{
    public class SerialDetector : MonoBehaviour
    {
        List<SerialPort> ports = new List<SerialPort>();
        byte[] buffer = new byte[64];
        long nextSwitch = 0;
        bool lastTypeSbus = false;

        public void Start()
        {
            StartAllPorts(false);
        }

        public void StartAllPorts(bool sbus)
        {
            string[] serialPorts = SerialPort.GetPortNames();
            foreach (string port in serialPorts)
            {
                SerialPort sp = null;
                try
                {
                    if (!sbus)
                    {
                        sp = new SerialPort(port, 115200, Parity.None, 8, StopBits.One);
                    }
                    else
                    {
                        sp = new SerialPort(port, 100000, Parity.Even, 8, StopBits.Two);
                    }
                    sp.Open();
                }
                catch
                {
                    //We don't care, this serial port can't be opened, look elsewhere.
                }
                if (sp != null && sp.IsOpen)
                {
                    Debug.Log($"Checking {sp.PortName} at {sp.BaudRate} baud");
                    ports.Add(sp);
                }
            }
            nextSwitch = DateTime.UtcNow.Ticks + TimeSpan.TicksPerSecond * 5;
        }

        public void StopAllPorts(SerialPort exclude)
        {
            foreach (SerialPort sp in ports)
            {
                try
                {
                    if (sp != exclude)
                    {
                        sp.Close();
                    }
                }
                catch
                {
                    //We don't care, this serial port can't be opened, look elsewhere.
                }
            }
            ports.Clear();
        }

        public void Update()
        {
            long currentTime = DateTime.UtcNow.Ticks;
            if (currentTime > nextSwitch)
            {
                StopAllPorts(null);
                lastTypeSbus = !lastTypeSbus;
                StartAllPorts(lastTypeSbus);

            }
            SerialPort excludePort = null;
            foreach (SerialPort sp in ports)
            {
                if (sp.BytesToRead >= 64)
                {
                    int type = DetectType(sp);
                    switch (type)
                    {
                        case 1:
                            GetComponent<WirelessRXMain>().StartIBUS(sp);
                            excludePort = sp;
                            break;
                        case 2:
                            GetComponent<WirelessRXMain>().StartSBUS(sp);
                            excludePort = sp;
                            break;
                        default:
                            break;
                    }
                }
            }
            if (excludePort != null)
            {
                StopAllPorts(excludePort);
                Destroy(this);
            }
        }

        public int DetectType(SerialPort sp)
        {
            sp.Read(buffer, 0, buffer.Length);
            //Check ibus first, this is a much more robust verification
            for (int i = 0; i < buffer.Length - 32; i++)
            {
                if (Checksum(i))
                {
                    return 1;
                }
            }
            //Check for sbus.
            for (int i = 0; i < buffer.Length - 25; i++)
            {
                if (buffer[i] == 0x0F && buffer[i + 24] == 0x00)
                {
                    return 2;
                }
            }
            return 0;
        }

        public bool Checksum(int startPos)
        {
            int length = buffer[startPos];
            if (length > 32)
            {
                return false;
            }
            if (startPos + length < 2)
            {
                return false;
            }
            int checksum = 0xFF;
            int storedChecksum = (buffer[startPos + length - 2] << 8) | (buffer[startPos + length - 1]);
            for (int i = startPos; i < startPos + length - 2; i++)
            {
                checksum -= buffer[i];
            }
            return checksum == storedChecksum;
        }
    }
}