using KLib.Signals.Enumerations;

namespace SpeechReception
{
    public class UserCustomization
    {
        public string testName;
        public float level;
        public float[] snr;

        public UserCustomization() { }
        public UserCustomization(string name, float level, float[] snr)
        {
            this.testName = name;
            this.level = level;
            this.snr = snr;
        }
        public UserCustomization(string name, float level, float snr)
        {
            this.testName = name;
            this.level = level;
            this.snr = new float[] { snr };
        }
    }
}