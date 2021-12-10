using System;
using System.IO;

namespace WirelessRXLib
{
    public class FileIO : IOInterface
    {
        private FileStream reader;
        private FileStream writer;

        public FileIO()
        {
            reader = new FileStream("input.txt", FileMode.Open, FileAccess.Read);
            File.Delete("output.txt");
            writer = new FileStream("output.txt", FileMode.Create, FileAccess.Write);
        }

        public int Available()
        {
            //Limit to 64 bytes to emulate serial
            int realBytesLeft = (int)(reader.Length - reader.Position);
            if (realBytesLeft > 64)
            {
                return 64;
            }
            return realBytesLeft;
        }

        public void Read(byte[] buffer, int length)
        {
            reader.Read(buffer, 0, length);
        }

        public void Write(byte[] buffer, int length)
        {
            writer.Write(buffer, 0, length);
        }
    }
}