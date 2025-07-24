using System.Collections.Generic;
using System.IO;

namespace MonitorManagerCS_GUI.Core
{
    internal class DisplayManager
    {
        public List<DisplayInfo> Displays { get; set; } = new List<DisplayInfo>();
        
        private string _outputFolder = "Temp";

        internal void GetDisplays()
        {
            string fileDirectory = _outputFolder;
            string fileName = "smonitors.txt";
            string filePath = Path.Combine(fileDirectory, fileName);

            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            Programs.RunProgram(Programs.controlMyMonitor, $"/smonitors {filePath}");

            Displays = ParseSMonitorsFile(filePath);
        }

        private List<DisplayInfo> ParseSMonitorsFile(string filePath)
        {
            var displays = new List<DisplayInfo>();
            int displayIndex = 0;

            string[] lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                //Split the line at the first colon
                var lineParts = line.Split(new[] { ':' }, 2);
                if (lineParts.Length != 2) continue;

                //The first part of the line is the variable identifier
                //The second part is the value of the variable in quotes
                var key = lineParts[0].Trim();
                var value = lineParts[1].Trim().Trim(new[] { '"' });

                if (key == "Monitor Device Name")
                {
                    displays.Add(new DisplayInfo
                    {
                        NumberID = value,
                        Index = displayIndex
                    });
                }
                if (key == "Monitor Name")
                {
                    displays[displayIndex].Name = value;
                }
                if (key == "Serial Number")
                {
                    displays[displayIndex].SerialNumber = value;
                }
                if (key == "Short Monitor ID")
                {
                    displays[displayIndex].ShortID = value;
                    displayIndex++;
                }
            }

            return displays;
        }
    }
}
