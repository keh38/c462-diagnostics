using System;
using System.IO;

namespace Launcher
{
    public static class FileLocations
    {
        public static string HardwareConfigFile { get { return Path.Combine(C462.Shared.SharedFileLocations.SharedFolder, "HardwareConfiguration.xml"); } }
    }
}