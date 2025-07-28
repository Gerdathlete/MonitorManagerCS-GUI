using System.IO;
using System.Reflection;

namespace MonitorManagerCS_GUI.Core
{
    internal static class Folders
    {
        internal readonly static string Config;
        internal readonly static string CMMonitorOutput;

        static Folders()
        {
            string exeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Config = Path.Combine(exeFolder, "Config");
            CMMonitorOutput = Path.Combine(exeFolder, "Temp");
        }
    }
}
