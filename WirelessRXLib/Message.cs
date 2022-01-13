namespace WirelessRXLib
{
	public class Message
	{
		public ushort[] channelsRaw = new ushort[18];
		public float[] channels = new float[18];
		public bool framelost;
		public bool failsafe;
	}
}