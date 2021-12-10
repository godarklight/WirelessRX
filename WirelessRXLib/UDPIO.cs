using System;
using System.Net;
using System.Net.Sockets;

namespace WirelessRXLib
{
    public class UDPIO : IOInterface
    {
        UdpClient udp;
        IPEndPoint endpoint;

        public UDPIO(int port)
        {
            udp = new UdpClient(new IPEndPoint(IPAddress.IPv6Any, port));
        }

        public int Available()
        {
            if (udp == null)
            {
                return 0;
            }
            return udp.Available;
        }

        public void Read(byte[] buffer, int length)
        {
            try
            {
                int bytesToRead = length;
                while (bytesToRead > 0)
                {
                    IPEndPoint any = new IPEndPoint(IPAddress.IPv6Any, 0);
                    byte[] receiveData = udp.Receive(ref any);
                    Array.Copy(receiveData, 0, buffer, length - bytesToRead, receiveData.Length);
                    endpoint = any;
                    bytesToRead -= bytesToRead;
                }
            }
            catch
            {
                Console.WriteLine("Disconnect");
                udp = null;
            }
        }

        public void Write(byte[] buffer, int length)
        {
            try
            {
                if (endpoint != null)
                {
                    udp.Send(buffer, length, endpoint);
                }
            }
            catch
            {
                Console.WriteLine("Disconnect");
                udp = null;
            }
        }
    }
}