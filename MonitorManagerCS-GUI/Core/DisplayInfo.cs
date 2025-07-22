using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorManagerCS_GUI.Core
{
    /// <summary>
    /// Stores info about a display (monitor)
    /// </summary>
    internal class DisplayInfo
    {
        public string DisplayNumber { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }
        public string ShortID { get; set; }
    }
}
