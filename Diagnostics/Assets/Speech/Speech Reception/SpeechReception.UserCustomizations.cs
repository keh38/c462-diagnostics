using System.Collections.Generic;
#if UNITY_METRO && !UNITY_EDITOR
using LegacySystem.IO;
#else
using System.IO;
#endif

namespace SpeechReception
{
    public class UserCustomizations
    {
        public string subjectID;
        public List<UserCustomization> customizations = new List<UserCustomization>();
        public bool keepBluetoothAlive = false;
        public float keepAliveAtten = float.NegativeInfinity;

        public UserCustomizations() { }

        public static UserCustomizations Initialize(string path, string id)
        {
            UserCustomizations uc = null;
            if (File.Exists(path))
                uc = KLib.FileIO.XmlDeserialize<UserCustomizations>(path);
            else
                uc = new UserCustomizations();

            uc.subjectID = id;
            return uc;
        }

        public void Set(string name, float level, float[] snr)
        {
            var c = customizations.Find(o => o.testName == name);
            if (c == null)
            {
                customizations.Add(new UserCustomization(name, level, snr));
            }
            else
            {
                c.level = level;
                c.snr = snr;
            }
        }

        public UserCustomization Get(string name)
        {
            return customizations.Find(o => o.testName == name);
        }
    }
}