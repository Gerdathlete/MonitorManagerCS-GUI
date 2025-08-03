using System;
using System.IO;
using System.Reflection;

namespace MonitorManagerCS_GUI.Core
{
    internal static class Folders
    {
        internal readonly static string Config;
        internal readonly static string ControlMyMonitor;
        internal readonly static string CMMonitorOutput;

        static Folders()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            Config = Path.Combine(baseDirectory, "Config");
            ControlMyMonitor = Path.Combine(baseDirectory, "ThirdParty", "ControlMyMonitor");
            CMMonitorOutput = Path.Combine(ControlMyMonitor, "Output");
        }
    }
}
