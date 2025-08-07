using System;

namespace MonitorManagerCS_GUI.Core
{
    public class TimedValue(double? hour, double? value)
    {
        public double? Hour { get; set; } = hour;
        public double? Value { get; set; } = value;

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
