using System;

namespace Turandot.Interactive
{
    public class UDPPacket
    {
        public int Status;
        public float[] Amplitudes;
        public float[] Values;
        public int[] Active;

        private int _arraySize = 10;
        public byte[] ByteArray;
        private int _packetSize;
        private int _sizeOfAmplitudes;
        private int _sizeOfValues;
        private int _sizeOfActive;
        
        public UDPPacket()
        {
            Amplitudes = new float[_arraySize];
            Values = new float[_arraySize];
            for (int k = 0; k < Values.Length; k++) Values[k] = float.NaN;

            Active = new int[_arraySize];
            for (int k = 0; k < Active.Length; k++) Active[k] = -1;

            _sizeOfAmplitudes = Amplitudes.Length * sizeof(float);
            _sizeOfValues = Values.Length * sizeof(float);
            _sizeOfActive = Active.Length * sizeof(int);
            _packetSize = sizeof(int) + _sizeOfAmplitudes + _sizeOfValues + _sizeOfActive;

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
            Buffer.BlockCopy(Values, 0, ByteArray, sizeof(int) + _sizeOfAmplitudes, _sizeOfValues);
            Buffer.BlockCopy(Active, 0, ByteArray, sizeof(int) + _sizeOfAmplitudes + _sizeOfValues, _sizeOfActive);
        }

        public void FromByteArray(byte[] byteArray)
        {
            Status = BitConverter.ToInt32(byteArray, 0);
            Buffer.BlockCopy(byteArray, sizeof(int), Amplitudes, 0, _sizeOfAmplitudes);
            Buffer.BlockCopy(byteArray, sizeof(int) + _sizeOfAmplitudes, Values, 0, _sizeOfValues);
            Buffer.BlockCopy(byteArray, sizeof(int) + _sizeOfAmplitudes + _sizeOfValues, Active, 0, _sizeOfActive);
        }

    }
}