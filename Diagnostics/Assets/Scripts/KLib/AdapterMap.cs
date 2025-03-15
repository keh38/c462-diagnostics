using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KLib
{
    public class AdapterMap
    {
        internal class AdapterSpec
        {
            public string jackName;
            public string modality;
            public string transducer;
            public string location;
            public string extra;
            public AdapterSpec(string jackName, string modality, string transducer, string location, string extra="")
            {
                this.jackName = jackName;
                this.modality = modality;
                this.transducer = transducer;
                this.location = location;
                this.extra = extra;
            }
        }

        private List<AdapterSpec> _items = new List<AdapterSpec>();

        public AdapterMap() { }

        public int NumChannels { get { return _items.Count; } }
        //public List<string> Devices { get { return _items.Select(x => x.device).ToList(); } }

        public static AdapterMap DefaultStereoMap()
        {
            var map = new AdapterMap();

            map.Add("Left", "Audio", "Headphones", "Left");
            map.Add("Right", "Audio", "Headphones", "Right");

            return map;
        }

        public static AdapterMap Default7point1Map()
        {
            var map = new AdapterMap();

            map.Add("Front-Left", "Audio", "Headphones", "Left");
            map.Add("Front-Right", "Audio", "Headphones", "Right");
            map.Add("Center", "Electric", "DSR.372", "Site 1", "50");
            map.Add("Subwoofer", "Electric", "DSR.000", "Site 2", "50");
            map.Add("Side-Left", "Haptic", "Haptic.1", "Site 1");
            map.Add("Side-Right", "Haptic", "Haptic.2", "Site 2");
            map.Add("Rear-Left", "", "", "");
            map.Add("Rear-Right", "", "", "");

            return map;
        }


        public void Add(string jackName, string modality, string location, string channel, string extra = "")
        {
            _items.Add(new AdapterSpec(jackName, modality, location, channel, extra));
        }

        public int GetAdapterIndex(string modality, string location)
        {
            int number = _items.FindIndex(o => o.modality == modality && o.location == location);
            if (number < 0)
                throw new Exception($"Adapter '{modality}.{location}' does not exist.");

            return number;
        }

        public List<string>GetLocations(string modality)
        {
            return _items.FindAll(x => x.modality.Equals(modality)).Select(y => y.location).ToList();
        }
    }
}
