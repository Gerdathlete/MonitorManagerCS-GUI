using Newtonsoft.Json;
using System.Drawing;
using System.Text.RegularExpressions;

namespace MonitorManagerCS_GUI.Core
{
    /// <summary>
    /// Stores info about a display (monitor)
    /// </summary>
    public partial class DisplayInfo
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

        private string _number = string.Empty;
        [JsonIgnore]
        public string Number
        {
            get
            {
                if (_number?.Length == 0)
                {
                    try
                    {
                        string[] numIDParts = NumberID.Split('\\');

                        string outputNumber = NumbersRegex().Replace(numIDParts[3], "");
                        int outputSubNumber = int.Parse(NumbersRegex().Replace(numIDParts[4], ""));

                        if (outputSubNumber > 0)
                        {
                            outputNumber += $".{outputSubNumber}";
                        }

                        _number = outputNumber;
                    }
                    catch
                    {
                        _number = "X";
                    }
                }

                return _number;
            }
        }

        public override string ToString()
        {
            return $"Display {Number}: {ShortID}";
        }

        [GeneratedRegex("[^0-9]")]
        private static partial Regex NumbersRegex();
    }
}
