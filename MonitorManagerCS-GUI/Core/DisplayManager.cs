using System.Collections.ObjectModel;
using System.IO;

namespace MonitorManagerCS_GUI.Core
{
    internal class DisplayManager
    {
        public ObservableCollection<DisplayInfo> Displays { get; set; } = new ObservableCollection<DisplayInfo>();

        internal void GetDisplays()
        {
            string fileDirectory = "Temp";
            string fileName = "smonitors.txt";
            string filePath = Path.Combine(fileDirectory, fileName);

            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            Programs.RunProgram(Programs.controlMyMonitor, $"/smonitors {filePath}");

            Displays.Clear();
            DisplayInfo display;
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
                var value = lineParts[1].Trim().Trim(new[] {'"'});

                if (key == "Monitor Device Name")
                {
                    Displays.Add(new DisplayInfo
                    {
                        NumberID = value,
                        Index = displayIndex
                    });
                }
                if (key == "Monitor Name")
                {
                    Displays[displayIndex].Name = value;
                }
                if (key == "Serial Number")
                {
                    Displays[displayIndex].SerialNumber = value;
                }
                if (key == "Short Monitor ID")
                {
                    Displays[displayIndex].ShortID = value;
                    displayIndex++;
                }
            }
        }
    }
}
