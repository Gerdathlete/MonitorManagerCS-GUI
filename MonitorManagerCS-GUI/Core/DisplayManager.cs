using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonitorManagerCS_GUI.Core
{
    internal class DisplayManager
    {
        public List<DisplayInfo> Displays { get; set; } = new List<DisplayInfo>();
        
        private string _outputFolder = "Temp";

        internal async Task<List<DisplayInfo>> GetDisplays()
        {
            string fileDirectory = _outputFolder;
            string fileName = "smonitors.txt";
            string filePath = Path.Combine(fileDirectory, fileName);

            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            await Programs.RunProgramAsync(Programs.controlMyMonitor, $"/smonitors {filePath}");

            Displays = ParseSMonitorsFile(filePath);

            //Generate tasks to pull each display's VCP info so that we can run them in parallel
            var tasks = Displays.Select(async display =>
            {
                var unsafeFileName = $"{display.ShortID}-SN{display.SerialNumber}.json";
                var jsonFileName = DataFormatter.GetSafeFileName(unsafeFileName);
                var jsonFilePath = Path.Combine(fileDirectory, jsonFileName);

                //Get the display's VCP codes
                //(/sjson generates a json file with the display's VCP codes given one of its identifiers)
                await Programs.RunProgramAsync(Programs.controlMyMonitor, $"/sjson {jsonFilePath} {display.NumberID}");

                //Read the file's text asynchronously with a stream reader
                string json;
                using (var reader = new StreamReader(jsonFilePath))
                {
                    json = await reader.ReadToEndAsync();
                }

                display.VCPCodes = JsonSerializer.Deserialize<List<VCPCode>>(json);
            }).ToList();

            await Task.WhenAll(tasks);

            return Displays;
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
