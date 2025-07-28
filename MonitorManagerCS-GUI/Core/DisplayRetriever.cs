using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonitorManagerCS_GUI.Core
{
    public static class DisplayRetriever
    {
        public static async Task<List<DisplayInfo>> GetDisplayList()
        {
            string fileDirectory = Folders.CMMonitorOutput;
            string fileName = "smonitors.txt";
            string filePath = Path.Combine(fileDirectory, fileName);

            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            await Programs.RunProgramAsync(Programs.controlMyMonitor, $"/smonitors {filePath}");

            var displays = ParseSMonitorsFile(filePath);

            return displays;
        }

        public static async Task<List<VCPCode>> GetVCPCodes(DisplayInfo display)
        {
            string fileDirectory = Folders.CMMonitorOutput;
            var fileName = display.ConfigFileName;
            var filePath = Path.Combine(fileDirectory, fileName);

            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            if (!File.Exists(filePath))
            {
                //Get the display's VCP codes
                //(/sjson generates a json file with the display's VCP codes given one of its
                //identifiers)
                await Programs.RunProgramAsync(Programs.controlMyMonitor,
                    $"/sjson {filePath} {display.NumberID}");
            }

            //Read the file's text asynchronously with a stream reader
            string json;
            using (var reader = new StreamReader(filePath))
            {
                json = await reader.ReadToEndAsync();
            }

            var vcpCodes = JsonSerializer.Deserialize<List<VCPCode>>(json);

            return vcpCodes;
        }

        private static List<DisplayInfo> ParseSMonitorsFile(string filePath)
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
