using System;
using System.Net;
using System.Net.Sockets;

using System.IO.Ports;

namespace WirelessRXLib
{
    public class TCPIO : IOInterface
    {
        TcpListener listener;
        TcpClient tcp;

        public TCPIO(int port)
        {
            listener = new TcpListener(new IPEndPoint(IPAddress.IPv6Any, port));
            listener.Server.DualMode = true;
            listener.Start();
            listener.BeginAcceptTcpClient(TCPConnect, null);
        }

        private void TCPConnect(IAsyncResult ar)
        {
            try
            {
                if (tcp != null)
                {
                    Console.WriteLine("Disconnect");
                    tcp.Close();
                }
            }
            catch
            {
            }
            tcp = listener.EndAcceptTcpClient(ar);
            Console.WriteLine($"Connnected");
            listener.BeginAcceptTcpClient(TCPConnect, null);
        }

        public int Available()
        {
            if (tcp == null)
            {
                return 0;
            }
            return tcp.Available;
        }

        public void Read(byte[] buffer, int length)
        {
            try
            {
                int bytesToRead = length;
                while (bytesToRead > 0)
                {
                    int bytesRead = tcp.GetStream().Read(buffer, length - bytesToRead, length);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Disconnect");
                        return;
                    }
                    bytesToRead -= bytesToRead;
                }
            }
            catch
            {
                Console.WriteLine("Disconnect");
                tcp = null;
            }
        }

        public void Write(byte[] buffer, int length)
        {
            try
            {
                tcp.GetStream().Write(buffer, 0, length);
            }
            catch
            {
                Console.WriteLine("Disconnect");
                tcp = null;
            }
        }
    }
}