using System.Collections.Generic;

namespace MonitorManagerCS_GUI.Core
{
    /// <summary>
    /// Stores info about a display (monitor)
    /// </summary>
    public class DisplayInfo
    {
        public string NumberID { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }
        public string ShortID { get; set; }
        public int Index { get; set; }
        public List<VCPCode> VCPCodes { get; set; }
    }
}
