namespace Pupillometry
{
    public class CalibrationSettings
    {
        public float targetSizeFactor = 60f;
        public float holeSizeFactor = 300f;
        public uint targetColor;
        public uint backgroundColor;
        public string calibrationType = "HV9";
        public int width = -1;
        public int height = -1;
        public UnityEngine.KeyCode keyCode = UnityEngine.KeyCode.None;
        public string finalPrompt = "";

        public CalibrationSettings()
        {
            //backgroundColor = (uint)KLib.Unity.ColorInt(0.75f, 0.75f, 0.75f, 1.0f);
            //targetColor = (uint)KLib.Unity.ColorInt(0f, 0f, 0f, 1.0f);
            keyCode = UnityEngine.KeyCode.None;
        }
    }
}