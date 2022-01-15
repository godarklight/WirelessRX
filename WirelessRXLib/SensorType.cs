namespace WirelessRXLib
{
    public enum IbusSensorType
    {
        NONE = 0x00,
        TEMPERATURE = 0x01,
        RPM_FLYSKY = 0x02,
        EXTERNAL_VOLTAGE = 0x03,
        CELL = 0x04, // Avg Cell voltage
        BAT_CURR = 0x05, // battery current A * 100
        FUEL = 0x06, //remaining battery percentage / mah drawn
        RPM = 0x07,
        CMP_HEAD = 0x08, //short Heading  0..360 deg, 0=north
        CLIMB_RATE = 0x09, //short m/s *100
        COG = 0x0a, //ushort Course over ground(NOT heading, but direction of movement) in degrees * 100, 0.0..359.99 degrees.
        GPS_STATUS = 0x0b,
        [SensorValueType(typeof(short))]
        ACC_X = 0x0c, // m/s *100
        [SensorValueType(typeof(short))]
        ACC_Y = 0x0d, // m/s *100
        [SensorValueType(typeof(short))]
        ACC_Z = 0x0e, // m/s *100
        [SensorValueType(typeof(short))]
        ROLL = 0x0f, // deg m/s *100
        [SensorValueType(typeof(short))]
        PITCH = 0x10, // deg *100
        [SensorValueType(typeof(short))]
        YAW = 0x11, // deg *100
        [SensorValueType(typeof(short))]
        VERTICAL_SPEED = 0x12, // m/s *100
        GROUND_SPEED = 0x13, // m/s *100
        GPS_DIST = 0x14, // home distance (m)
        ARMED = 0x15,
        FLIGHT_MODE = 0x16,
        PRES = 0x41,
        ODO1 = 0x7c,
        ODO2 = 0x7d,
        SPE = 0x7e, // km/h

		[SensorValueType(typeof(int))]
		GPS_LAT = 0x80, //int32 WGS84 in degrees * 1E7
		[SensorValueType(typeof(int))]
		GPS_LON = 0x81, //int32 WGS84 in degrees * 1E7
		[SensorValueType(typeof(int))]
		GPS_ALT = 0x82, //int32 GPS alt m*100
		[SensorValueType(typeof(int))]
		ALT = 0x83, //int32 Alt m*100
		[SensorValueType(typeof(int))]
		ALT_MAX = 0x84, //int32 MaxAlt m*100
        ALT_FLYSKY = 0xf9, // Altitude short in m
        UNKNOWN = 0xff
    }
    public enum CrsfSensorType
    {
        GPS = 0x02,
        BATTERY_SENSOR = 0x08,
    }
}