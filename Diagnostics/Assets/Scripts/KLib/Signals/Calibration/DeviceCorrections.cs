using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if UNITY_METRO && !UNITY_EDITOR
using LegacySystem.IO;
#else
using System.IO;
#endif

namespace KLib.Signals.Calibration
{
    public class DeviceCorrections
    {
        public class DeviceCorrection
        {
            public string name;
            public float value;
            public DeviceCorrection() { }
            public DeviceCorrection(string name, float value)
            {
                this.name = name;
                this.value = value;
            }
        }
        public List<DeviceCorrection> items = new List<DeviceCorrection>();

        public static DeviceCorrections Restore(string path)
        {
            DeviceCorrections dc = new DeviceCorrections();
            if (File.Exists(path))
            {
                dc = KLib.FileIO.XmlDeserialize<DeviceCorrections>(path);
            }
            return dc;
        }

        public void SetCorrection(string name, float value)
        {
            var dc = items.Find(o => o.name.Equals(name));
            if (dc != null)
                dc.value = value;
            else
            {
                items.Add(new DeviceCorrection(name, value));
            }
        }

        public float GetCorrection(string name)
        {
            var dc = items.Find(o => o.name.Equals(name));
            return dc == null ? 0 : dc.value;
        }

    }
}
