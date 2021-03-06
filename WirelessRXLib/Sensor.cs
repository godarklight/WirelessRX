using System;

namespace WirelessRXLib
{
    public class Sensor
    {
        public readonly SensorType type;
        public readonly int length;
        private readonly Type sensorValueType;
        private readonly Func<int> inputValue;

        public Sensor(SensorType type, Func<int> inputValue)
        {
            this.type = type;
            this.inputValue = inputValue;
            this.sensorValueType = typeof(ushort);
            this.length = 2;
            foreach (Attribute att in typeof(SensorType).GetMember(type.ToString())[0].GetCustomAttributes(false))
            {
                SensorValueType svt = att as SensorValueType;
                sensorValueType = svt.type;
                if (sensorValueType == typeof(int))
                {
                    length = 4;
                }
            }
        }

        public int WriteValue(int id, byte[] buffer)
        {
            buffer[0] = (byte)(4 + length);
            buffer[1] = (byte)(0xA0 | id);
            if (sensorValueType == typeof(short))
            {
                BitConverter.GetBytes((short)inputValue()).CopyTo(buffer, 2);
            }
            if (sensorValueType == typeof(ushort))
            {
                BitConverter.GetBytes((ushort)inputValue()).CopyTo(buffer, 2);
            }
            if (sensorValueType == typeof(int))
            {
                BitConverter.GetBytes(inputValue()).CopyTo(buffer, 2);
            }
            return 4 + length;
        }
    }
}