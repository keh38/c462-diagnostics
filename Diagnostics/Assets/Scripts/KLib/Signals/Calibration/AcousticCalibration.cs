using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

            string localCalFolder = folder;
            string calName = transducer;
            if (!string.IsNullOrEmpty(speaker)) calName += "_" + speaker;

            // 1. Side-specific calibration stored in local folder?
            string fn = Path.Combine(localCalFolder, calName + ".xml");

            if (!File.Exists(fn))
                throw new Exception($"Calibration not found: {Path.GetFileNameWithoutExtension(fn)}");

            return FileIO.XmlDeserialize<AcousticCalibration>(fn);
        }
    }

}
