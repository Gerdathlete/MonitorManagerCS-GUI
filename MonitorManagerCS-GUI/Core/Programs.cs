using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorManagerCS_GUI.Core
{
    internal static class Programs
    {
        internal const string controlMyMonitor = @"C:\PortableApplications\controlmymonitor\ControlMyMonitor.exe";

        /// <summary>
        /// Runs the given program and returns the text output
        /// </summary>
        /// <param name="program"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static string RunProgram(string program, string args)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = program;
                process.StartInfo.Arguments = args;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();

                string textOutput = process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                return textOutput;
            }
        }
    }
}
