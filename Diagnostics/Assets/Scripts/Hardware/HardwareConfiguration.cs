using System.Collections.Generic;

namespace KLib
{
    public class HardwareConfiguration
    {
        public string CurrentAdapterMap { set; get; }
        public List<AdapterMap> AdapterMaps { set; get; }
        public string SyncComPort { set; get; }
        public string LEDComPort { set; get; }
        public int ScreenBrightness { set; get; }

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
            config.LEDComPort = "";
            config.ScreenBrightness = -1;
            return config;
        }

        public bool UsesDigitimer()
        {
            return GetSelectedMap().Items.Find(x => x.transducer.StartsWith("DS8R")) != null;
        }
    }
}