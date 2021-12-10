#pragma warning disable CA1416
//Serial warnings for ios and android

using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using WirelessRXLib;

namespace Ibus
{
    class Program
    {
        private static long startupTime = DateTime.UtcNow.Ticks;
        private static IOInterface io;
        private static byte[] sendBuffer = new byte[64];
        public static void Main(string[] args)
        {
            SetupIO(args);

            Sender sender = new Sender(io);
            SbusHandler handler = new SbusHandler(MessageEvent, sender);
            SbusDecoder decoder = new SbusDecoder(handler);

            bool running = true;
            byte[] buffer = new byte[64];
            while (running)
            {
                int bytesAvailable = 0;
                while ((bytesAvailable = io.Available()) > 0)
                {
                    int bytesRead = bytesAvailable;
                    if (bytesRead > buffer.Length)
                    {
                        bytesRead = buffer.Length;
                    }
                    io.Read(buffer, bytesRead);
                    decoder.Decode(buffer, bytesRead);
                }
                //FileIO has run out of data, quit.
                if (io is FileIO)
                {
                    running = false;
                }
                Thread.Sleep(1);
            }
        }

        private static void MessageEvent(Message m)
        {
            Console.WriteLine($"message {m.channels[0].ToString("0.00")}, {m.channels[1].ToString("0.00")}, {m.channels[2].ToString("0.00")}, {m.channels[3].ToString("0.00")}, FS: {m.failsafe}");
        }

        private static void SetupIO(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "file")
                {
                    io = new FileIO();
                }
                if (args[0] == "tcp")
                {
                    io = new TCPIO(5867);
                }
                if (args[0] == "serial")
                {
                    if (args.Length > 1)
                    {
                        io = new SerialIO(args[1]);
                    }
                    else
                    {
                        string[] serialPorts = SerialPort.GetPortNames();
                        if (serialPorts.Length == 1)
                        {
                            io = new SerialIO(serialPorts[0]);
                        }
                        else
                        {
                            foreach (string port in serialPorts)
                            {
                                Console.WriteLine($"Available serial ports: {port}");
                            }
                        }
                    }
                }
                if (args[0] == "udp")
                {
                    io = new UDPIO(5687);
                }
            }
            if (io == null)
            {
                io = new SerialIO("/dev/ttyUSB0");
            }
        }
    }
}
