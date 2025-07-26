using System;

namespace Audiometer
{
    public class UDPPacket
    {
        public int Status;
        public int[] Amplitudes;

        private int _arraySize = 10;
        public byte[] ByteArray;
        private int _packetSize;
        private int _sizeOfAmplitudes;
        
        public UDPPacket()
        {
            Amplitudes = new int[_arraySize];
            _sizeOfAmplitudes = Amplitudes.Length * sizeof(float);
            _packetSize = sizeof(int) + _sizeOfAmplitudes;

            ByteArray = new byte[_packetSize];
        }

        public void SetAmplitudes(float[] amplitudes)
        {
            for (int k = 0; k < amplitudes.Length; k++) Amplitudes[k] = amplitudes[k] > 0 ? 1 : 0;
        }

        public void UpdateByteArray()
        {
            Buffer.BlockCopy(BitConverter.GetBytes(Status), 0, ByteArray, 0, sizeof(Int32));
            Buffer.BlockCopy(Amplitudes, 0, ByteArray, sizeof(int), _sizeOfAmplitudes);
        }

        public void FromByteArray(byte[] byteArray)
        {
            Status = BitConverter.ToInt32(byteArray, 0);
            Buffer.BlockCopy(byteArray, sizeof(int), Amplitudes, 0, _sizeOfAmplitudes);
        }

    }
}