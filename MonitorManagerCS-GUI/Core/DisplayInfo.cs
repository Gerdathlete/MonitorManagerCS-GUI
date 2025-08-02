using Newtonsoft.Json;
using System.Drawing;
using System.Text.RegularExpressions;

namespace MonitorManagerCS_GUI.Core
{
    /// <summary>
    /// Stores info about a display (monitor)
    /// </summary>
    public class DisplayInfo
    {
        //From ControlMyMonitor
        [JsonIgnore]
        public string NumberID { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }
        public string ShortID { get; set; }

        //From Screen.AllScreens
        [JsonIgnore]
        public Rectangle Bounds { get; set; }
        [JsonIgnore]
        public bool IsPrimary { get; set; }

        //Derived properties
        public string LongID { get => $"{ShortID}-SN{SerialNumber}"; }
        [JsonIgnore]
        public string ConfigFileName { get => DataFormatter.GetSafeFileName(LongID) + ".json"; }

        private int? _number = null;
        public int Number
        {
            get
            {
                if (_number == null)
                {
                    try
                    {
                        var numIDParts = NumberID.Split('\\');

                        int outputNumber = int.Parse(Regex.Replace(numIDParts[3], "[^0-9]", ""));
                        int outputSubNumber = int.Parse(Regex.Replace(numIDParts[4], "[^0-9]", ""));

                        if (outputSubNumber > 0)
                        {
                            outputNumber += outputSubNumber * 100;
                        }

                        _number = outputNumber;
                    }
                    catch
                    {
                        _number = 0;
                    }
                }

                return (int)_number;
            }
        }

        public override string ToString()
        {
            return $"Display {Number}: {ShortID}";
        }
    }
}
