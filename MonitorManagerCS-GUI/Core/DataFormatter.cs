using System.IO;
using System.Text.RegularExpressions;

namespace MonitorManagerCS_GUI.Core
{
    public static class DataFormatter
    {
        /// <summary>
        /// Gets a string in the format "hh:mm AM/PM" for the given time since 12 AM
        /// </summary>
        /// <param name="hour"></param>
        /// <returns></returns>
        public static string GetReadableTime(double hour)
        {
            int hourInt = (int)hour;
            int minute = (int)(hour % 1 * 60);

            return GetReadableTime(hourInt, minute);
        }
        /// <summary>
        /// Gets a string in the format "hh:mm AM/PM" for the given time since 12 AM
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        public static string GetReadableTime(int hour, int minute)
        {
            string AMPM = (hour % 24) < 12 ? "AM" : "PM";

            hour %= 12;
            hour = hour == 0 ? 12 : hour;

            string hourStr = hour.ToString();

            //Add a leading zero to the minute if needed
            string minuteStr = minute < 10 ? $"0{minute}" : minute.ToString();

            return $"{hourStr}:{minuteStr} {AMPM}";
        }

        public static string GetMinMaxString(int min, int max)
        {
            return $"{min}-{max}";
        }

        public static string GetSafeFileName(string fileName)
        {
            return Regex.Replace(fileName,
                $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", "_");
        }
    }
}
