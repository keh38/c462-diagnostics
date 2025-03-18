using System.Collections.Generic;

namespace KLib
{
    public class HardwareConfiguration
    {
        public string CurrentAdapterMap { set; get; }
        public List<AdapterMap> AdapterMaps { set; get; }
        public string SyncComPort { set; get; }

        public HardwareConfiguration() { }

        public AdapterMap GetSelectedMap()
        {
            return AdapterMaps.Find(x => x.Name.Equals(CurrentAdapterMap));
        }
  
        public static HardwareConfiguration GetDefaultConfiguration()
        {
            var config = new HardwareConfiguration();
            config.AdapterMaps = new List<AdapterMap>();
            config.AdapterMaps.Add(AdapterMap.DefaultStereoMap());
            config.AdapterMaps.Add(AdapterMap.Default7point1Map());
            config.CurrentAdapterMap = config.AdapterMaps[0].Name;
            config.SyncComPort = "";
            return config;
        }
    }
}