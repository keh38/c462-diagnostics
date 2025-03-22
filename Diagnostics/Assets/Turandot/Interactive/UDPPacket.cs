using System;

namespace Turandot.Interactive
{
    public class UDPPacket
    {
        public int Status;
        public float[] Amplitudes;
        public float[] Values;

        private int _arraySize = 10;
        public byte[] ByteArray;
        private int _packetSize;
        private int _sizeOfAmplitudes;
        private int _sizeOfValues;
        
        public UDPPacket()
        {
            Amplitudes = new float[_arraySize];
            Values = new float[_arraySize];

            _sizeOfAmplitudes = Amplitudes.Length * sizeof(float);
            _sizeOfValues = Values.Length * sizeof(float);
            _packetSize = sizeof(int) + _sizeOfAmplitudes + _sizeOfValues;
            ByteArray = new byte[_packetSize];
        }

        public void SetAmplitudes(float[] amplitudes)
        {
            for (int k = 0; k < amplitudes.Length; k++) Amplitudes[k] = amplitudes[k];
        }

        public void UpdateByteArray()
        {
            Buffer.BlockCopy(BitConverter.GetBytes(Status), 0, ByteArray, 0, sizeof(Int32));
            Buffer.BlockCopy(Amplitudes, 0, ByteArray, sizeof(int), _sizeOfAmplitudes);
            Buffer.BlockCopy(Amplitudes, 0, ByteArray, sizeof(int) + _sizeOfAmplitudes, _sizeOfValues);
        }

        public void FromByteArray(byte[] byteArray)
        {
            Status = BitConverter.ToInt32(byteArray, 0);
            Buffer.BlockCopy(byteArray, sizeof(int), Amplitudes, 0, _sizeOfAmplitudes);
            Buffer.BlockCopy(byteArray, sizeof(int) + _sizeOfAmplitudes, Amplitudes, 0, _sizeOfValues);
        }

    }
}