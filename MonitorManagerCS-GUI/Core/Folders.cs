using System;
using System.IO;

namespace MonitorManagerCS_GUI.Core
{
    internal static class Folders
    {
        internal readonly static string Config;
        internal readonly static string ControlMyMonitor;
        internal readonly static string CMMonitorOutput;

        private readonly static string[] _directories;

        static Folders()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            Config = Path.Combine(baseDirectory, "Config");
            ControlMyMonitor = Path.Combine(baseDirectory, "ThirdParty", "ControlMyMonitor");
            CMMonitorOutput = Path.Combine(ControlMyMonitor, "Output");

            _directories = new string[]
            { baseDirectory, Config, ControlMyMonitor, CMMonitorOutput };
        }

        /// <summary>
        /// Tries to read and write to every directory in Folders. Outputs an exception if failed.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns> True if successful, false if failed.</returns>
        public static bool CanReadAndWrite(out Exception ex)
        {
            ex = null;

            try
            {
                foreach (var directory in _directories)
                {
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    string testFile = Path.Combine(directory, "testFile");

                    if (File.Exists(testFile)) File.Delete(testFile);

                    File.WriteAllText(testFile, "Testing read/write permissions.");

                    File.ReadAllText(testFile);

                    File.Delete(testFile);
                }
            }
            catch (Exception exception)
            {
                ex = exception;
                return false;
            }

            return true;
        }
    }
}
