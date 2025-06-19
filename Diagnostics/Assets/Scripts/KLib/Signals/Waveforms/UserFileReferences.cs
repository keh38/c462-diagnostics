using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using KLib;

namespace KLib.Signals.Waveforms
{
    public class UserFileReferences
    {
        public class Entry
        {
            public string transducer="";
            public string destination="";
            public string units;
            public float value;
        }

        public List<Entry> entries = new List<Entry>();

        public UserFileReferences() { }

        public static UserFileReferences Read(string path)
        {
            var ufr = new UserFileReferences();

            var folder = Path.GetDirectoryName(path);
            var filename = Path.GetFileNameWithoutExtension(path);
            int idash = filename.IndexOf('-');
            if (idash > 0) filename = filename.Substring(0, idash);

            var refPath = Path.Combine(folder, filename + ".xml");
            if (!File.Exists(refPath))
            {
                refPath = Path.Combine(folder, Path.GetFileNameWithoutExtension(path) + ".xml");
            }

            if (File.Exists(path))
            {
                ufr = FileIO.XmlDeserialize<UserFileReferences>(refPath);
            }
            return ufr;
        }

        public float GetReference(string units)
        {
            return GetReference(units, "", "");
        }
        public float GetReference(string units, string transducer, string destination)
        {
            float value = float.NaN;

            //var r = entries.Find(x => x.transducer.Equals(transducer) && x.destination.Equals(destination) && x.units.Equals(units));
            var r = entries.Find(x => x.transducer.Equals(transducer) && x.units.Equals(units));

            if (r != null)
            {
                value = r.value;
            }

            return value;
        }

    }
}