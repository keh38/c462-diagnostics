namespace KLib.Wave
{
    public class WaveFormat
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort nBitsPerSample;
        public ushort cbSize;

        public WaveFormat() { }
    }
}