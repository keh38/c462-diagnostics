using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace KLib
{
    public class AdapterMap
    {
        public class AdapterSpec
        {
            public string jackName;
            public string modality;
            public string transducer;
            public string location;
            public string extra;
            public AdapterSpec() { }
            public AdapterSpec(string jackName, string modality, string transducer, string location, string extra = "")
            {
                this.jackName = jackName;
                this.modality = modality;
                this.transducer = transducer;
                this.location = location;
                this.extra = extra;
            }
        }

        public class Endpoint
        {
            public int index;
            public string transducer;
            public string location;
            public string extra;
            public Endpoint() { }
            public override string ToString()
            {
                return $"{transducer}.{location}";
            }
        }
        public string Name { set; get; }
        public List<AdapterSpec> Items = new List<AdapterSpec>();
        private string _audioTransducer = "";

        public AdapterMap() { }

        public int NumChannels { get { return Items.Count; } }
        public string AudioTransducer
        {
            get
            {
                return _audioTransducer;
            }
            set
            {
                _audioTransducer = value;
                foreach (var i in Items.FindAll(x => x.modality == "Audio"))
                {
                    i.transducer = _audioTransducer;
                }
            }
        }


        public static AdapterMap DefaultStereoMap()
        {
            var map = new AdapterMap();
            map.Name = "Stereo";

            map.Add("Left", "Audio", "Headphones", "Left");
            map.Add("Right", "Audio", "Headphones", "Right");

            return map;
        }

        public static AdapterMap Default7point1Map()
        {
            var map = new AdapterMap();
            map.Name = "7.1";

            map.Add("Front-Left", "Audio", "Headphones", "Left");
            map.Add("Front-Right", "Audio", "Headphones", "Right");
            map.Add("Center", "Electric", "DSR372", "Site 1", "50");
            map.Add("Subwoofer", "Electric", "DSR000", "Site 2", "50");
            map.Add("Side-Left", "Haptic", "Haptic1", "Site 1");
            map.Add("Side-Right", "Haptic", "Haptic2", "Site 2");
            map.Add("Rear-Left", "", "", "");
            map.Add("Rear-Right", "", "", "");

            return map;
        }


        public void Add(string jackName, string modality, string location, string channel, string extra = "")
        {
            Items.Add(new AdapterSpec(jackName, modality, location, channel, extra));
        }

        public List<string> GetLocations(string modality)
        {
            return Items.FindAll(x => x.modality.Equals(modality)).Select(y => y.location).ToList();
        }

        private int GetAdapterIndex(string modality, string location)
        {
            int index = Items.FindIndex(o => o.modality == modality && o.location == location);
            if (index < 0)
                throw new Exception($"Adapter '{modality}.{location}' does not exist.");

            return index;
        }

        public Endpoint GetEndpoint(string modality, string location)
        {
            var index = GetAdapterIndex(modality, location);

            var ep = new Endpoint()
            {
                index = index,
                transducer = Items[index].transducer,
                location = location,
                extra = Items[index].extra
            };
            return ep;
        }
    }
}
