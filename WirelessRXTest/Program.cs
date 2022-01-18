#pragma warning disable CA1416
//Serial warnings for ios and android

using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using WirelessRXLib;

namespace WirelessRXTest
{
    public class Program
    {
        public static IbusSensor[] IbusSensors
        {
            private set;
            get;
        }
        static bool crsf=false, ibus=false, sbus = false;
   		private CrsfHandler handler;
        private static long startupTime = DateTime.UtcNow.Ticks;
        private static IOInterface io;
        private static IDecoder decoder;
        private static SerialDetector detector;
        private static long startTime;
        private static byte[] buffer = new byte[64];
        public static void Main(string[] args)
        {
            startTime = DateTime.UtcNow.Ticks;
            long exitTime = startTime + 20 * TimeSpan.TicksPerSecond;
            SetupIO(args);

            if (io == null)
            {
                detector = new SerialDetector(Console.WriteLine, DetectEvent);
                while (io == null)
                {
                    long currentTime = DateTime.UtcNow.Ticks;
                    if (currentTime > exitTime)
                    {
                        Console.WriteLine($"Failed to find any serial ports");
                        detector.Stop();
                        return;
                    }
                    Thread.Sleep(100);
                }
                if (decoder == null)
                {
                    return;
                }
            }
            else
            {

                while (io.Available() < 64)
                {
                    long currentTime = DateTime.UtcNow.Ticks;
                    if (currentTime > exitTime)
                    {
                        Console.WriteLine($"Failed to find any data on your selected IO type {io.GetType()}");
                        return;
                    }
                    Thread.Sleep(100);
                }
                io.Read(buffer, buffer.Length);
                int type = SerialDetector.DetectType(buffer);
                DetectEvent(type, null);
            }

            CrsfSender sender = new CrsfSender(io);
            CrsfHandler handler = new CrsfHandler(MessageEvent,sender);
            bool running = true;
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
                    Thread.Sleep(10);
                    if(crsf)
                        {
                            handler.HandleMessage(TestSensorValue2());
                        }
                }
                //FileIO has run out of data, quit.
                if (io is FileIO)
                {
                    running = false;
                }
                Thread.Sleep(1);
            }
        }

        private static void DetectEvent(int type, SerialPort sp)
        {
            if (sp != null)
            {
                io = new SerialIO(sp);
            }
            CrsfSender crsfsender = new CrsfSender(io);
            IbusSender ibussender = new IbusSender(io);
            switch (type)
            {
                case 1:
                    {
                        Console.WriteLine($"Starting IBUS Decoder");
                        IbusSensors = new IbusSensor[16];
                        IbusSensors[1] = new IbusSensor(IbusSensorType.GPS_ALT, TestSensorValue);
                        IbusHandler handler = new IbusHandler(MessageEvent, IbusSensors, ibussender);
                        decoder = new IbusDecoder(handler);
                        ibus=true;
                    }
                    break;
                case 2:
                    {
                        Console.WriteLine($"Starting SBUS Decoder");
                        SbusHandler handler = new SbusHandler(MessageEvent, ibussender);
                        decoder = new SbusDecoder(handler);
                        sbus=true;
                    }
                    break;
                case 3:
                    {
                        Console.WriteLine($"Starting CRSF Decoder");
                        CrsfHandler handler = new CrsfHandler(MessageEvent,crsfsender);
                        decoder = new CrsfDecoder(handler);
                        crsf=true;
                    }
                    break;
                default:
                    Console.WriteLine($"Unknown serial type {type}, not decoding");
                    break;
            }
        }

        private static void MessageEvent(Message m)
        {
            for (int i = 0; i <= 15; i++)
            {
                Console.Write($"ch{i}: {m.channels[i].ToString("0.00")}   ");
            }
            Console.WriteLine($"Failsafe:{m.failsafe}");

        }

        private static int TestSensorValue()
        {
            long currentTime = DateTime.UtcNow.Ticks;
            long timeDelta = (currentTime - startTime) / (100 * TimeSpan.TicksPerMillisecond);
            return (int)timeDelta;
        }
        private static byte[] TestSensorValue2()
        {
            byte[] timeDelta = new byte[11];
            long currentTime = DateTime.UtcNow.Ticks;
            int E = (int)((currentTime - startTime) / (100 * TimeSpan.TicksPerMillisecond));
            timeDelta[0]=0xEA;
            timeDelta[1]=(byte)timeDelta.Length;
            timeDelta[2]=0x08;
            timeDelta[3]=(byte)(0xFF00 & E);
            timeDelta[4]=(byte)(0x00FF & E);
            timeDelta[5]=0;
            timeDelta[6]=0;
            timeDelta[7]=0;
            timeDelta[8]=0;
            timeDelta[9]=0;
            timeDelta[10]=100;
            return timeDelta;
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
                    Console.WriteLine("Listening on TCP port 5867");
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
                    Console.WriteLine("Listening on UDP port 5867");
                    io = new UDPIO(5687);
                }
            }
        }
    }
}