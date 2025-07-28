using Newtonsoft.Json;
using System.Collections.Generic;

namespace MonitorManagerCS_GUI.Core
{
    /// <summary>
    /// Stores info about a display (monitor)
    /// </summary>
    public class DisplayInfo
    {
        [JsonIgnore]
        public string NumberID { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }
        public string ShortID { get; set; }
        public string LongID { get => $"{ShortID}-SN{SerialNumber}"; }
        [JsonIgnore]
        public string ConfigFileName { get => DataFormatter.GetSafeFileName(LongID) + ".json"; }
    }
}
