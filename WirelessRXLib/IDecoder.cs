namespace WirelessRXLib
{
	public interface IDecoder
	{
		void Decode(byte[] bytes, int length);
	}
}