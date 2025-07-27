using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorManagerCS_GUI.Core
{
    public class TimedValue
    {
        internal double? Hour { get; set; }
        internal double? Value { get; set; }

        public TimedValue(double? hour, double? value)
        {
            Hour = hour;
            Value = value;
        }

        public override string ToString()
        {
            string value = Value.ToString();
            if (Value != null)
            {
                value = Math.Round((double)Value, 4).ToString();
            }

            return $"({Hour}, {value})";
        }
    }
}
