using System.Text.RegularExpressions;

namespace MonitorManagerCS_GUI
{
    public static class DataInterpreter
    {
        public static int[] ParseReadableTime(string time)
        {
            //Example input: "7:08 PM"
            //Returns [19, 8]
            byte hour = 0;
            byte minute = 0;
            string buffer = "";
            for (int i = 0; i < time.Length; i++)
            {
                char c = time[i];

                //If we reach a colon, the content of the buffer is the hour
                if (c == ':')
                {
                    //Store the buffer value as the hour
                    hour = byte.Parse(buffer);
                    //Empty the buffer, go to the next character
                    buffer = "";
                    continue;
                }

                //If we reach a space, the content of the buffer is the minute, possibly with a leading zero
                if (c == ' ')
                {
                    //If there is a leading zero (ie. buffer == "0#"), remove it
                    if (time[i - 2] == '0')
                    {
                        buffer = buffer.Substring(1);
                    }
                    //Store the buffer value as the minute
                    minute = byte.Parse(buffer);

                    //Put the last two characters into the buffer
                    buffer = time.Substring(i + 1);

                    //If its PM, but not 12 PM, add 12 to the hour
                    if (buffer == "PM")
                    {
                        if (hour != 12) hour += 12;
                    }
                    //If its 12 AM, the hour is 0. Ex: "12:03 AM" -> hour = 0
                    else if (hour == 12)
                    {
                        hour = 0;
                    }
                    //If its AM, and not 12 AM, the values are already correct

                    //We are done!
                    break;
                }

                //Add the character to the buffer
                buffer += c;
            }
            return new int[] { hour, minute };
        }

        public static Regex readableTimeRegex = new Regex(@"^([1-9]|1[0-2]):[0-5][0-9] (AM|PM)$");

        public static Regex blueLightRegex = new Regex(@"^[0-4]-[0-4]$");

        public static Regex minMaxRegex = new Regex(@"^(?:0|[1-9][0-9]?|100)-(?:0|[1-9][0-9]?|100)$");

        /// <summary>
        /// Turns a min-max string into two ints
        /// (e.g. "2-5" -&gt; {2, 5})
        /// </summary>
        /// <param name="minMaxString"></param>
        /// <returns></returns>
        public static int[] ParseMinMaxString(string minMaxString)
        {
            //Example input: 15-100
            string[] minMax = minMaxString.Split('-');
            return new int[] { int.Parse(minMax[0]), int.Parse(minMax[1]) };

        }
    }
}
