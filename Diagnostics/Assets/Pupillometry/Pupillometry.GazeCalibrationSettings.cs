using System;
using System.Xml.Serialization;

namespace Pupillometry
{
    public class GazeCalibrationSettings
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int BackgroundColor { set; get; }

        public float TargetSizeFactor { get; set; }
        public float HoleSizeFactor { get; set; }
        public int TargetColor { set; get; }

        public string CalibrationType { get; set; }
        public UnityEngine.KeyCode KeyCode { get; set; }

        public GazeCalibrationSettings()
        {
            Width = -1;
            Height = -1;
            CalibrationType = "HV9";

            TargetSizeFactor = 60;
            HoleSizeFactor = 300;

            TargetColor = KLib.ColorTranslator.ColorInt(0.75f, 0.75f, 0.75f, 1);
            BackgroundColor = KLib.ColorTranslator.ColorInt(0, 0, 0, 1);

            KeyCode = UnityEngine.KeyCode.Space;
        }
    }
}