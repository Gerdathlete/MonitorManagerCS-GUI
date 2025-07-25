using System;
using System.Diagnostics;
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

        internal static async Task<string> RunProgramAsync(string program, string args)
        {
            var tcs = new TaskCompletionSource<string>();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = program,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                },
                EnableRaisingEvents = true
            };

            string output = "";

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    output += e.Data + Environment.NewLine;
                }
            };

            process.Exited += (sender, e) =>
            {
                tcs.SetResult(output);
                process.Dispose();
            };

            process.Start();
            process.BeginOutputReadLine();

            return await tcs.Task;
        }
    }
}
