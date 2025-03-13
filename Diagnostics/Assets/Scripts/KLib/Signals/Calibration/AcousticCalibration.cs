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

        public static AcousticCalibration Load(string transducer, string destination)
        {
            if (string.IsNullOrEmpty(transducer))
            {
                transducer = "DefaultCal";
            }
#if FIXME
            string localCalFolder = Path.Combine(DataFileLocations.BasicResourcesFolder, "Calibration");
#else
            string localCalFolder = "";
#endif
            string calName = transducer;
            if (destination == "Left" || destination == "Right")
            {
                calName += "_" + destination;
            }

            // 1. Side-specific calibration stored in local folder?
            string fn = System.IO.Path.Combine(localCalFolder, calName + ".xml");
            if (File.Exists(fn))
            {
                return FileIO.XmlDeserialize<AcousticCalibration>(fn);
            }

            // 2. Use the default diotic calibration stored in the Resources
            //Debug.Log(calName);
            return FileIO.XmlDeserializeFromTextAsset<AcousticCalibration>(calName);
        }

        public static AcousticCalibration LoadPC(string transducer, string speaker, string folder)
        {
            string localCalFolder = folder;
            string calName = transducer;
            if (!string.IsNullOrEmpty(speaker)) calName += "_" + speaker;

            // 1. Side-specific calibration stored in local folder?
            string fn = System.IO.Path.Combine(localCalFolder, calName + ".xml");
            if (!File.Exists(fn)) return null;
                //throw new Exception("Calibration not found: " + fn);

            return FileIO.XmlDeserialize<AcousticCalibration>(fn);
        }
    }

}
