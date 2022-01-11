using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO.Ports;

namespace WirelessRXLib
{
    public class SerialDetector
    {
        List<SerialPort> ports = new List<SerialPort>();
        bool detecting = true;
        byte[] buffer = new byte[64];
        SerialPort detectedPort;
        Action<string> Log;
        Action<int, SerialPort> DetectEvent;

        public SerialDetector(Action<string> log, Action<int, SerialPort> detectEvent)
        {
            this.Log = log;
            this.DetectEvent = detectEvent;
            DetectLoop();
        }

        private async void DetectLoop()
        {

            bool lastTypeSbus = false;
            while (detecting && detectedPort == null)
            {
                StartAllPorts(lastTypeSbus);
                //Check for 5 seconds
                for (int i = 0; i < 50; i++)
                {
                    Detect();
                    if (detectedPort != null)
                    {
                        break;
                    }
                    await Task.Delay(100);
                }
                StopAllPorts();
                lastTypeSbus = !lastTypeSbus;
            }
        }

        public void Stop()
        {
            detecting = false;
        }

        private void StartAllPorts(bool sbus)
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
                    Log($"Checking {sp.PortName} at {sp.BaudRate} baud");
                    ports.Add(sp);
                }
            }
        }

        private void StopAllPorts()
        {
            foreach (SerialPort sp in ports)
            {
                try
                {
                    if (sp != detectedPort)
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

        private void Detect()
        {
            foreach (SerialPort sp in ports)
            {
                if (sp.BytesToRead >= 64)
                {
                    int type = DetectType(sp);
                    if (type > 0)
                    {
                        Log($"Found {sp.PortName} at {sp.BaudRate} baud, type: {type}");
                        detectedPort = sp;
                        DetectEvent(type, sp);
                    }
                }
            }
        }

        private int DetectType(SerialPort sp)
        {
            sp.Read(buffer, 0, buffer.Length);
            return DetectType(buffer);
        }

        public static int DetectType(byte[] chunk)
        {
            //Check ibus first, this is a much more robust verification
            for (int i = 0; i < chunk.Length - 32; i++)
            {
                if (Checksum(i, chunk))
                {
                    return 1;
                }
            }
            //Check for sbus.
            for (int i = 0; i < chunk.Length - 25; i++)
            {
                if (chunk[i] == 0x0F && chunk[i + 24] == 0x00)
                {
                    return 2;
                }
            }
            //Check for crsf
            for (int i = 0; i < chunk.Length - 25; i++)
            {
                if (chunk[i] == 0xC8 && chunk[i+1] == 0x18)
                {
                    return 3;
                }
            }
            return 0;
        }

        private static bool Checksum(int startPos, byte[] chunk)
        {
            int length = chunk[startPos];
            if (length > 32)
            {
                return false;
            }
            if (startPos + length < 2)
            {
                return false;
            }
            int checksum = 0xFFFF;
            int storedChecksum = (chunk[startPos + length - 2]) | (chunk[startPos + length - 1] << 8);
            for (int i = startPos; i < startPos + length - 2; i++)
            {
                checksum -= chunk[i];
            }
            return checksum == storedChecksum;
        }
    }
}