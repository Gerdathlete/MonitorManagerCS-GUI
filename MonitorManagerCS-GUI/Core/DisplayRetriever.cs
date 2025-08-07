using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonitorManagerCS_GUI.Core
{
    public static class DisplayRetriever
    {
        public static readonly List<string> InvalidVCPCodes =
        [
            //Preset Functions
            "08", "04", "06", "05", "0A", "B0", "00",
            //Image Adjustment
            "0E", "1C", "1E", "1F", "3E", "56", "58", "73", "74", "75", "7C", "88", "A2", "A4",
            "A5", "A6", "A7",
            //Display Control
            "C8", "C9", "C6", "AC", "DB", "CA", "CC", "B5", "B4", "DF", "AE",
            //Geometry
            "95", "96", "97", "98", "DA",
            //Miscellaneous Functions
            "02", "03", "52", "76", "78", "B2", "B6", "C2", "C3", "C4", "C6", "C7", "C8", "CE",
            "D2", "DE", "8D", "94"
        ];

        public static async Task<List<DisplayInfo>> GetDisplayList()
        {
            string fileDirectory = Folders.CMMonitorOutput;
            const string fileName = "smonitors.txt";
            string filePath = Path.Combine(fileDirectory, fileName);

            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            await Programs.RunProgramAsync(Programs.ControlMyMonitor, $"/smonitors \"{filePath}\"");

            var displays = ParseSMonitorsFile(filePath);

            foreach (var display in displays)
            {
                IEnumerable<Screen> screens = Screen.AllScreens
                    .Where(s => display.NumberID.Contains(s.DeviceName));

                if (screens.Any())
                {
                    var screen = screens.First();

                    display.Bounds = screen.Bounds;
                    display.IsPrimary = screen.Primary;
                }
            }

            return displays;
        }

        public static async Task<List<VCPCode>> GetVCPCodes(DisplayInfo display)
        {
            var vcpCodes = await GetRawVCPCodes(display);

            return vcpCodes.Filtered();
        }
        public static async Task<List<VCPCode>> GetRawVCPCodes(DisplayInfo display)
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
                await Programs.RunProgramAsync(Programs.ControlMyMonitor,
                    $"/sjson \"{filePath}\" \"{display.NumberID}\"");
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

        private static List<VCPCode> Filtered(this List<VCPCode> vcpCodes) => [.. vcpCodes
                .Where(vcp =>
                vcp.MaximumValue != "0" //A max value of 0 indicates an unsupported code
                && vcp.IsWritable
                && !InvalidVCPCodes.Contains(vcp.Code) //Isn't in the invalid code list
                ).GroupBy(vcp => vcp.Code).Select(g => g.First())];

        private static List<DisplayInfo> ParseSMonitorsFile(string filePath)
        {
            var displays = new List<DisplayInfo>();
            int displayIndex = 0;

            string[] lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                //Split the line at the first colon
                var lineParts = line.Split([':'], 2);
                if (lineParts.Length != 2) continue;

                //The first part of the line is the variable identifier
                //The second part is the value of the variable in quotes
                var key = lineParts[0].Trim();
                var value = lineParts[1].Trim().Trim(['"']);

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
