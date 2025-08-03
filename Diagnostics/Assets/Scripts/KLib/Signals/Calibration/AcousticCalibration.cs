using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;

namespace KLib.Signals.Calibration
{
    [Serializable]
    public class AcousticCalibration
    {
        public float df_Hz;
        public float[] dBSPL_Vrms;

        public static AcousticCalibration Load(string folder, string transducer, string speaker)
        {
            if (string.IsNullOrEmpty(transducer))
                throw new Exception("Must specify a transducer to load an acoustic calibration");

            // NEW!!!
            var calPath = Path.Combine(FileLocations.LocalResourceFolder("Calibration"),
                $"{transducer}.{speaker}.calib");
            Debug.Log($"local cal file = {calPath}");
            if (File.Exists(calPath))
            {
                return AcousticCalibration.ReadCFTSCalibrationFile(calPath);
            }

            string localCalFolder = folder;
            string calName = transducer;
            if (!string.IsNullOrEmpty(speaker)) calName += "_" + speaker;

            // 1. Side-specific calibration stored in local folder?
            string fn = Path.Combine(localCalFolder, calName + ".xml");

            if (!File.Exists(fn))
                throw new Exception($"Calibration not found: {Path.GetFileNameWithoutExtension(fn)}");

            return FileIO.XmlDeserialize<AcousticCalibration>(fn);
        }

        public static AcousticCalibration ReadCFTSCalibrationFile(string filename)
        {
            var lines = File.ReadAllLines(filename);

            if (!lines[0].StartsWith("[ACOUSTIC CALIBRATION]"))
                throw new Exception("Invalid acoustic calibration file format");

            int dataIndex = -1;
            for (int k=0; k<lines.Length; k++)
            {
                if (lines[k].StartsWith("Freq(Hz)"))
                {
                    dataIndex = k + 1;
                    break;
                }
            }

            if (dataIndex < 0)
                throw new Exception("Invalid acoustic calibration file format");

            int npts = lines.Length - dataIndex + 1;
            float[] freq = new float[npts];
            float[] mag = new float[npts];

            int index = 0;
            for (int k = dataIndex; k < lines.Length; k++)
            {
                string[] columns = lines[k].Split('\t', StringSplitOptions.RemoveEmptyEntries);

                if (k == dataIndex)
                {
                    freq[index] = 0;
                    mag[index] = float.Parse(columns[1]);
                    index++;
                }

                freq[index] = float.Parse(columns[0]);
                mag[index] = float.Parse(columns[1]);

                index++;
            }

            System.Array.Resize(ref freq, index);
            System.Array.Resize(ref mag, index);

            var acal = new AcousticCalibration();
            acal.df_Hz = freq[2] - freq[1];
            acal.dBSPL_Vrms = mag;

            return acal;
        }
    }

}
