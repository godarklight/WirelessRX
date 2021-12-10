namespace WirelessRXLib
{
    public interface IOInterface
    {
        int Available();
        void Read(byte[] buffer, int length);
        void Write(byte[] buffer, int length);
    }
}