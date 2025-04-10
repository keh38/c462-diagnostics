using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Pupillometry
{
    public class GazeCalibrationSettings
    {
        public int Width { get; set; }
        private bool ShouldSerializeWidth() { return false; }

        public int Height { get; set; }
        private bool ShouldSerializeHeight() { return false; }

        public float TargetSizeFactor { get; set; }
        private bool ShouldSerializeTargetSizeFactor() { return false; }

        public float HoleSizeFactor { get; set; }
        private bool ShouldSerializeHoleSizeFactor() { return false; }


        //[Category("Appearance")]
        [DisplayName("Target Color")]
        [XmlIgnore]
        public System.Drawing.Color TargetWindowsColor
        {
            get { return System.Drawing.Color.FromArgb(TargetColor); }
            set { TargetColor = value.ToArgb(); }
        }
        private bool ShouldSerializeTargetWindowsColor() { return false; }

        [Browsable(false)]
        public int TargetColor { set; get; }

        [DisplayName("Background Color")]
        [XmlIgnore]
        public System.Drawing.Color BackgroundWindowsColor
        {
            get { return System.Drawing.Color.FromArgb(BackgroundColor); }
            set { BackgroundColor = value.ToArgb(); }
        }
        private bool ShouldSerializeBackgroundWindowsColor() { return false; }

        [Browsable(false)]
        public int BackgroundColor { set; get; }

        public string CalibrationType { get; set; }
        private bool ShouldSerializeCalibrationType() { return false; }

        public UnityEngine.KeyCode keyCode = UnityEngine.KeyCode.None;

        public GazeCalibrationSettings()
        {
            Width = -1;
            Height = -1;
            CalibrationType = "HV9";

            TargetSizeFactor = 60;
            HoleSizeFactor = 300;

            TargetColor = KLib.ColorTranslator.ColorInt(0.75f, 0.75f, 0.75f, 1);
            BackgroundColor = KLib.ColorTranslator.ColorInt(0, 0, 0, 1);

            keyCode = UnityEngine.KeyCode.None;
        }
    }
}