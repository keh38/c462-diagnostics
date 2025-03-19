using System;
using System.IO;

namespace Launcher
{
    public static class FileLocations
    {
        public static readonly string RootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EPL", "HTS");
        public static string HardwareConfigFile { get { return Path.Combine(RootFolder, "HardwareConfiguration.xml"); } }
    }
}