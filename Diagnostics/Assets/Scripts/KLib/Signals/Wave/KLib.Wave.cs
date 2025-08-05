using UnityEngine;
using System.Collections.Generic;

using System;
#if UNITY_METRO && !UNITY_EDITOR
using LegacySystem.IO;
#else
using System.IO;
#endif

using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoBuf;

namespace KLib.Wave
{
    public class WaveFile
    {
        string _path;
        WaveFormat _fmt = null;
        WaveData _data = null;

        Dictionary<string, float> _references = null;

        public WaveFile()
        {
        }

        public uint SamplesPerChannel
        {
            get { return (_data != null) ? _data.samplesPerChannel : 0; }
        }

        public int NumChannels
        {
            get { return _fmt.nChannels; }
        }

        public float[] GetChannel(int ichan)
        {
            if (_data == null || _data.samples == null || ichan > _data.samples.GetUpperBound(0))
                return null;

            return _data.samples[ichan];
        }

        public uint SamplingRate
        {
            get { return _fmt.nSamplesPerSec; }
        }

        public float Duration
        {
            get { return (float)_data.samplesPerChannel / (float)_fmt.nSamplesPerSec; }
        }

        public Dictionary<string, float> References
        {
            get { return _references; }
        }

        void Clear()
        {
            _fmt = null;
            _data = null;
        }

        public void Read(string path, bool metadataOnly=false, int npts=0)
        {
            uint skip;
            Clear();

            _path = path;

            using (System.IO.Stream s = File.OpenRead(_path))
            {
                // Find the first RIFF chunk:
                WaveChunk chunk = ReadWaveChunk(s);
                if (chunk.ID != "RIFF")
                    throw new System.Exception("Not a WAVE file");

                // Verify that RIFF file is WAVE data type:
                CheckRiffType(s, "WAVE");

                // Find optional chunks, and don't stop till <data-ck> found:
                bool endOfFile = false;

                while (!endOfFile)
                {
                    chunk = ReadWaveChunk(s);
                    switch (chunk.ID.ToLower())
                    {
                        case "end of file":
                            endOfFile = true;
                            break;
                        case "fmt":
                            _fmt = ReadWaveFmt(s, chunk);
                            break;
                        case "data":
                            _data = ReadWaveData(s, chunk, metadataOnly);
                            break;
                        case "epl":
                            _references = ReadEPLChunk(s, chunk);
                            break;
                        default:
                            skip = chunk.size + chunk.size % 2;
                            s.Seek(skip, System.IO.SeekOrigin.Current);
                            break;
                    }

                }

            }
        }

        WaveChunk ReadWaveChunk(System.IO.Stream s)
        {
            byte[] buffer = new byte[4];
            int nread = s.Read(buffer, 0, 4);

            string id;
            if (nread == 4)
            {
                id = new string(System.Text.Encoding.UTF8.GetChars(buffer)).TrimEnd(new char[] { ' ' });
            }
            else if (s.Position == s.Length)
            {
                id = "end of file";
            }
            else
                throw new System.Exception("Truncated chunk header found - possibly not a WAV file");

            uint size = ReadUInt(s);

            return new WaveChunk(id, size);
        }

        void CheckRiffType(System.IO.Stream s, string ftype)
        {
            string riffType = ReadString(s, 4);
            if (riffType.ToLower() != ftype.ToLower())
                throw new System.Exception("Not a WAVE file.");
        }

        WaveFormat ReadWaveFmt(System.IO.Stream s, WaveChunk chunk)
        {
            string errMsg = "Error reading <wave-fmt> chunk.";
            WaveFormat fmt = new WaveFormat();

            uint nbytes = 14;
            uint origPos = (uint)s.Position;

            if (chunk.size < nbytes)
                throw new System.Exception(errMsg);

            fmt.wFormatTag = ReadUShort(s);
            fmt.nChannels = ReadUShort(s);
            fmt.nSamplesPerSec = ReadUInt(s);
            fmt.nAvgBytesPerSec = ReadUInt(s);
            fmt.nBlockAlign = ReadUShort(s);

            if (fmt.wFormatTag == 1)
            {
                ReadFmtPCM(s, chunk, fmt);
            }

            uint totalBytes = chunk.size + chunk.size % 2;
            uint rbytes = totalBytes - ((uint)s.Position - origPos);
            if (rbytes > 0)
            {
                s.Seek(rbytes, System.IO.SeekOrigin.Current);
            }

            return fmt;
        }

        void ReadFmtPCM(System.IO.Stream s, WaveChunk chunk, WaveFormat fmt)
        {
            string err = "Error reading PCM < wave - fmt > chunk.";

            // There had better be a bits / sample field:
            uint nbytes = 14; // # of bytes already read in <wave-format> header

            if (chunk.size < nbytes + 2)
                throw new System.Exception(err);

            fmt.nBitsPerSample = ReadUShort(s);
            nbytes += 2;

            // Are there any additional fields present ?
            if (chunk.size > nbytes)
            {
                // See if the "cbSize" field is present.If so, grab the data:
                if (chunk.size >= nbytes + 2)
                {
                    // we have the cbSize ushort in the file:
                    fmt.cbSize = ReadUShort(s);
                    nbytes += 2;
                }

                // Simply skip any remaining stuff - we don't know what it is:
                uint totalBytes = chunk.size + chunk.size % 2;
                uint rbytes = totalBytes - nbytes;
                if (rbytes > 0)
                {
                    s.Seek(rbytes, System.IO.SeekOrigin.Current);
                }
            }
        }

        WaveData ReadWaveData(System.IO.Stream s, WaveChunk chunk, bool metadataOnly)
        {
            if (_fmt == null)
                throw new System.Exception("Corrupt WAV file: found audio data before format information.");

            if (_fmt.wFormatTag != 1)
                throw new System.Exception("Data compression format not supported.");

            WaveData d = new WaveData();

            uint bytesPerSample = (uint)Mathf.CeilToInt(_fmt.nBitsPerSample / 8f);
            uint totalBytes = chunk.size;
            d.totalSamples = totalBytes / bytesPerSample;
            d.samplesPerChannel = d.totalSamples / _fmt.nChannels;

            if (bytesPerSample != 2)
                throw new System.Exception("Can only parse 16-bit wave data.");

            byte[] buffer = new byte[totalBytes];
            s.Read(buffer, 0, (int)totalBytes);

            d.samples = new float[_fmt.nChannels][];
            for (int k = 0; k < _fmt.nChannels; k++)
            {
                d.samples[k] = new float[d.samplesPerChannel];
            }


            if (!metadataOnly)
            {
                int offset = 0;
                int ichan = 0;
                int isample = 0;
                for (int k = 0; k < d.totalSamples; k++)
                {
                    var intSampleValue = BitConverter.ToInt16(buffer, offset);
                    d.samples[ichan][isample] = intSampleValue / 32768.0f;

                    if (++ichan == _fmt.nChannels)
                    {
                        ichan = 0;
                        ++isample;
                    }

                    offset += (int)bytesPerSample;
                }
            }
            return d;
        }

        private Dictionary<string, float> ReadEPLChunk(System.IO.Stream s, WaveChunk chunk)
        {
            Dictionary<string, float> r = new Dictionary<string, float>();

            uint nbytes = 0;

            while (nbytes < chunk.size)
            {
                uint nchar = ReadUShort(s);
                string name = ReadString(s, (int)nchar);
                float val = (float)ReadDouble(s);

                nbytes += 2 + nchar + 8;

                name = name.Substring(0, (int) nchar);
                r.Add(name, val);

                //Debug.Log("EPL chunk: " + name + " = " + val);
            }

            return r;
        }

        public static void Write(float[] data, uint Fs, int nbits, string path)
        {
            ushort bytes_per_sample = (ushort) Mathf.CeilToInt(nbits / 8);
            uint total_samples = (uint) data.Length;
            uint total_bytes = total_samples * bytes_per_sample;

#if !UNITY_METRO || UNITY_EDITOR
            using (System.IO.Stream s = File.OpenWrite(path))
#else
            using (System.IO.Stream s = File.Create(path))
#endif
            {
                WriteWaveChunk(s, new WaveChunk("RIFF", 36 + total_bytes));
                WriteWaveChunk(s, new WaveChunk("WAVE", 0));
                WriteWaveChunk(s, new WaveChunk("fmt ", 16));
                WriteWaveFormat(s, 1, 1, Fs, (ushort)nbits, bytes_per_sample * Fs, bytes_per_sample);
                WriteWaveChunk(s, new WaveChunk("data", total_bytes));

                float m = Mathf.Pow(2, nbits - 1);
                float b = 0;
                short[] scaled = new short[data.Length];
                for (int k = 0; k < data.Length; k++) scaled[k] = (short)Mathf.Round(data[k] * m + b);
    
                byte[] buffer = new byte[2 * scaled.Length];
                Buffer.BlockCopy(scaled, 0, buffer, 0, buffer.Length);
                s.Write(buffer, 0, buffer.Length);
            }
        }

        static void WriteWaveChunk(System.IO.Stream s, WaveChunk chunk)
        {
            byte[] b = System.Text.Encoding.UTF8.GetBytes(chunk.ID);
            s.Write(b, 0, 4);

            if (chunk.size > 0)
            {
                b = BitConverter.GetBytes(chunk.size);
                s.Write(b, 0, b.Length);
            }
        }

        static void WriteWaveFormat(System.IO.Stream s, ushort tag, ushort nchan, uint Fs, ushort bitsPerSample, uint bytesPerSec, ushort nBlockAlign)
        {
            s.Write(BitConverter.GetBytes(tag), 0, 2);
            s.Write(BitConverter.GetBytes(nchan), 0, 2);
            s.Write(BitConverter.GetBytes(Fs), 0, 4);
            s.Write(BitConverter.GetBytes(bytesPerSec), 0, 4);
            s.Write(BitConverter.GetBytes(nBlockAlign), 0, 2);
            s.Write(BitConverter.GetBytes(bitsPerSample), 0, 2);
        }
        public static string ReadString(Stream s, int len)
        {
            byte[] buffer = new byte[len];
            int nread = s.Read(buffer, 0, len);
            return new string(System.Text.Encoding.UTF8.GetChars(buffer));
        }
        public static uint ReadUInt(Stream s)
        {
            byte[] buffer = new byte[4];
            int nread = s.Read(buffer, 0, 4);
            return BitConverter.ToUInt32(buffer, 0);
        }
        public static ushort ReadUShort(Stream s)
        {
            byte[] buffer = new byte[2];
            int nread = s.Read(buffer, 0, 2);
            return BitConverter.ToUInt16(buffer, 0);
        }
        public static double ReadDouble(Stream s)
        {
            byte[] buffer = new byte[8];
            int nread = s.Read(buffer, 0, 8);
            return BitConverter.ToDouble(buffer, 0);
        }

    } // WaveFile class
} // KLib.Wave namespace