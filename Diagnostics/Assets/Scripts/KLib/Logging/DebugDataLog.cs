using System;
using System.IO;
using C462.Shared;

namespace KLib.Logging
{
    public static class DebugDataLog
    {
        private static string _filePath = null;

        public static void Add(string name, float[] data)
        {
            byte[] dataBytes = new byte[data.Length * sizeof(float)];
            Buffer.BlockCopy(data, 0, dataBytes, 0, dataBytes.Length);

            AppendToLog(name, "FloatArray", data.Length, dataBytes);
        }

        private static void AppendToLog(string name, string dataType, int numData, byte[] data)
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                CreateLog();
            }

            byte[] dataTypeBytes = System.Text.Encoding.UTF8.GetBytes(dataType);
            byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(name);

            using (FileStream fs = new FileStream(_filePath, FileMode.Append, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(dataTypeBytes.Length);
                bw.Write(dataTypeBytes);
                bw.Write(nameBytes.Length);
                bw.Write(nameBytes);
                bw.Write(numData);
                bw.Write(data);

                bw.Close();
                fs.Close();
            }
        }

        private static void CreateLog()
        {
            _filePath = Path.Combine(SharedFileLocations.HtsSubjectDataFolder, $"DebugDataLog-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.bin");
            using (FileStream fs = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write("DebugDataLog");

                bw.Close();
                fs.Close();
            }

        }
    }
}