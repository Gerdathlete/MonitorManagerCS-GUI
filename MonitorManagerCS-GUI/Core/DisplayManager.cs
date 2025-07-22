using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorManagerCS_GUI.Core
{
    internal class DisplayManager
    {
        public ObservableCollection<DisplayInfo> Displays { get; set; }

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

            string[] lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (line.StartsWith("Monitor Device Name:"))
                {
                    
                }
                if (line.StartsWith("Monitor Name:"))
                {

                }
                if (line.StartsWith("Serial Number:"))
                {

                }
                if (line.StartsWith("Short Monitor ID:"))
                {

                }
            }
        }
    }
}
