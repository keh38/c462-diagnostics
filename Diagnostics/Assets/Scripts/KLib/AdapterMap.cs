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
            public string modality;
            public string location;
            public string channel;
            public AdapterSpec(string modality, string location, string channel)
            {
                this.modality = modality;
                this.location = location;
                this.channel = channel;
            }
        }

        private List<AdapterSpec> _items = new List<AdapterSpec>();

        public AdapterMap() { }

        public int NumChannels { get { return _items.Count; } }
        //public List<string> Devices { get { return _items.Select(x => x.device).ToList(); } }

        public void Add(string modality, string location, string channel)
        {
            _items.Add(new AdapterSpec(modality, location, channel));
        }

        public int GetAdapterNumber(string adapter)
        {
            var parts = adapter.Split('.');
            return GetAdapterNumber(parts[0], parts[1]);
        }

        public int GetAdapterNumber(string modality, string location)
        {
            int number = _items.FindIndex(o => o.modality == modality && o.location == location);
            if (number < 0)
                throw new Exception($"Adapter '{modality}.{location}' does not exist.");

            return number;
        }

    }
}
