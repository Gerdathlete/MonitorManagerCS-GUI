using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MonitorManagerCS_GUI
{
    //This program uses ControlMyMonitor v1.37 https://www.nirsoft.net/utils/control_my_monitor.html

    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading;

    public class StartupSettings
    {
        public static string StartupSettingsFilePath = Path.Combine(Environment.CurrentDirectory, "MMStartupSettings.json");
        //By default, the settings are located in the base directory.
        public static string DefaultSettingsFilePath = Path.Combine(Environment.CurrentDirectory, "MMSettings.json");
        public string SettingsFilePath { get; set; }

        public StartupSettings()
        {
            SettingsFilePath = DefaultSettingsFilePath;
        }

        /// <summary>
        /// Writes the startup settings file based on the current startup settings
        /// </summary>
        public void WriteStartupSettingsFile()
        {
            // Serialize the startup settings to JSON
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, options);

            // Write the JSON string to the file
            File.WriteAllText(StartupSettingsFilePath, json);
        }

        /// <summary>
        /// Reads the startup settings file and updates the startup settings accordingly
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="FormatException"></exception>
        public void ReadStartupSettingsFile()
        {
            // If the file doesn't exist, throw an exception
            if (!File.Exists(StartupSettingsFilePath))
            {
                throw new FileNotFoundException($"No startup settings file found at {StartupSettingsFilePath}");
            }

            // Read the JSON string from the file
            string json = File.ReadAllText(StartupSettingsFilePath);

            // Deserialize the JSON string into a StartupSettings object. Throw an exception if this fails
            StartupSettings newSettings = JsonSerializer.Deserialize<StartupSettings>(json)
                                    ?? throw new FormatException("Invalid file format: Unable to deserialize startup settings.");

            // Update the properties of the current instance with the values from the deserialized settings
            // Get all properties of the Settings class
            PropertyInfo[] properties = typeof(StartupSettings).GetProperties();

            // Loop through each property and update the current instance with the values from the deserialized settings
            foreach (PropertyInfo property in properties)
            {
                // Check if the property has both a getter and a setter
                if (property.CanRead && property.CanWrite)
                {
                    // Get the corresponding value from the deserialized settings
                    object value = property.GetValue(newSettings);

                    // Perform a null check before setting the value of the property
                    if (value != null)
                    {
                        // Set the value of the property in the current instance
                        property.SetValue(this, value);
                    }
                }
            }
        }
    }

    public class DefaultSettings
    {
        //When to start lowering brightness
        public const byte DimStartHour = 18;
        public const byte DimStartMinute = 0;

        //When to stop lowering brightness
        public const byte DimEndHour = 22;
        public const byte DimEndMinute = 00;

        //When to start raising brightness
        public const byte BrightStartHour = 6;
        public const byte BrightStartMinute = 0;

        //When to stop raising brightness
        public const byte BrightEndHour = 9;
        public const byte BrightEndMinute = 0;

        //Monitor min and max brightnesses
        public const byte CenterMaxBrightness = 60;
        public const byte CenterMinBrightness = 0;

        public const byte LeftMaxBrightness = 100;
        public const byte LeftMinBrightness = 20;

        public const byte RightMaxBrightness = 100;
        public const byte RightMinBrightness = 20;

        //Center monitor blue light filter settings
        public const byte MinBlueLightFilter = 0;
        public const byte MaxBlueLightFilter = 3;

        //Time between brightness checks in seconds (interpolation quality)
        public const int BrightCheckTime = 108;

        //Monitor IDs
        public const string MonitorLeft = "\\\\.\\DISPLAY3\\Monitor0"; //"E5LMTF111018"
        public const string MonitorCenter = "\\\\.\\DISPLAY1\\Monitor0"; //"R8LMTF100594"
        public const string MonitorRight = "\\\\.\\DISPLAY2\\Monitor0"; //"DALMTF069312"
    }

    public class Settings
    {
        #region Properties
        //When to start lowering brightness
        public byte DimStartHour { get; set; }
        public byte DimStartMinute { get; set; }

        //When to stop lowering brightness
        public byte DimEndHour { get; set; }
        public byte DimEndMinute { get; set; }

        //When to start raising brightness
        public byte BrightStartHour { get; set; }
        public byte BrightStartMinute { get; set; }

        //When to stop raising brightness
        public byte BrightEndHour { get; set; }
        public byte BrightEndMinute { get; set; }

        //Monitor min and max brightnesses
        public byte CenterMaxBrightness { get; set; }
        public byte CenterMinBrightness { get; set; }

        public byte LeftMaxBrightness { get; set; }
        public byte LeftMinBrightness { get; set; }

        public byte RightMaxBrightness { get; set; }
        public byte RightMinBrightness { get; set; }

        //Center monitor blue light filter settings
        public byte MinBlueLightFilter { get; set; }
        public byte MaxBlueLightFilter { get; set; }

        //Time between brightness checks in milliseconds (interpolation quality)
        public int BrightCheckTime { get; set; }

        //Monitor IDs
        public string MonitorLeft { get; set; }
        public string MonitorCenter { get; set; }
        public string MonitorRight { get; set; }
        #endregion

        public Settings()
        {
            //When to start lowering brightness
            DimStartHour = DefaultSettings.DimStartHour;
            DimStartMinute = DefaultSettings.DimStartMinute;

            //When to stop lowering brightness
            DimEndHour = DefaultSettings.DimEndHour;
            DimEndMinute = DefaultSettings.DimEndMinute;

            //When to start raising brightness
            BrightStartHour = DefaultSettings.BrightStartHour;
            BrightStartMinute = DefaultSettings.BrightStartMinute;

            //When to stop raising brightness
            BrightEndHour = DefaultSettings.BrightEndHour;
            BrightEndMinute = DefaultSettings.BrightEndMinute;

            //Monitor min and max brightnesses
            CenterMaxBrightness = DefaultSettings.CenterMaxBrightness;
            CenterMinBrightness = DefaultSettings.CenterMinBrightness;

            LeftMaxBrightness = DefaultSettings.LeftMaxBrightness;
            LeftMinBrightness = DefaultSettings.LeftMinBrightness;

            RightMaxBrightness = DefaultSettings.RightMaxBrightness;
            RightMinBrightness = DefaultSettings.RightMinBrightness;

            //Center monitor blue light filter settings
            MinBlueLightFilter = DefaultSettings.MinBlueLightFilter;
            MaxBlueLightFilter = DefaultSettings.MaxBlueLightFilter;

            //Time between brightness checks in milliseconds (interpolation quality)
            BrightCheckTime = DefaultSettings.BrightCheckTime;

            //Monitor IDs
            MonitorLeft = DefaultSettings.MonitorLeft;
            MonitorCenter = DefaultSettings.MonitorCenter;
            MonitorRight = DefaultSettings.MonitorRight;
        }

        /// <summary>
        /// Writes the current settings to the given file path
        /// </summary>
        /// <param name="filePath"></param>
        public void WriteSettingsFile(string filePath)
        {
            // Serialize the current instance of Settings to JSON
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, options);

            // Write the JSON string to the file
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Reads settings from the given file path and updates the settings accordingly
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="FormatException"></exception>
        public void ReadSettingsFile(string filePath)
        {
            // If the file doesn't exist, throw an exception
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"No settings file found at {filePath}");
            }

            // Read the JSON string from the file
            string json = File.ReadAllText(filePath);

            // Deserialize the JSON string into a Settings object. Throw an exception if this fails
            Settings newSettings = JsonSerializer.Deserialize<Settings>(json)
                                    ?? throw new FormatException("Invalid file format: Unable to deserialize settings.");

            // Update the properties of the current instance with the values from the deserialized settings
            // Get all properties of the Settings class
            PropertyInfo[] properties = typeof(Settings).GetProperties();

            // Loop through each property and update the current instance with the values from the deserialized settings
            foreach (PropertyInfo property in properties)
            {
                // Check if the property has both a getter and a setter
                if (property.CanRead && property.CanWrite)
                {
                    // Get the corresponding value from the deserialized settings
                    object value = property.GetValue(newSettings);

                    // Perform a null check before setting the value of the property
                    if (value != null)
                    {
                        // Set the value of the property in the current instance
                        property.SetValue(this, value);
                    }
                }
            }
        }
    }

    public class Programs
    {
        public const string controlMyMonitor = @"C:\PortableApplications\controlmymonitor\ControlMyMonitor.exe";
    }

    public class VCPCodes
    {
        // Settings (VCP codes)
        public const string input = "60";
        public const string brightness = "10";
        public const string blueLightFilter = "E6";

    }

    public class Sources
    {
        // Input source values
        public const int vga = 1;
        public const int dvi = 3;
        public const int hdmi = 17;
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static DateTime currentTime;
        private static DateTime dimStartTime;
        private static DateTime dimEndTime;
        private static DateTime brightStartTime;
        private static DateTime brightEndTime;

        public static void MonitorService(Settings settings, CancellationToken cancellationToken)
        {
            Debug.WriteLine("[Monitor Service] Monitor service started");
            //Perform actions based on settings
            int secondsElapsed = 0;
            UpdateBrightness(settings);
            while (true)
            {
                //Every second check if the task should be canceled
                Thread.Sleep(1000);
                if (cancellationToken.IsCancellationRequested) break;
                secondsElapsed++;

                //If the number of seconds that have elapsed is enough to update brightness...
                if (secondsElapsed >= settings.BrightCheckTime)
                {
                    //Update the brightness
                    UpdateBrightness(settings);
                    //Reset the seconds elapsed counter
                    secondsElapsed = 0;
                }
            }
            Debug.WriteLine("[Monitor Service] Monitor service ended");
        }

        public static StartupSettings InitStartupSettings()
        {
            StartupSettings startupSettings = new StartupSettings();

            //If there isn't a startup settings file, make one
            if (!File.Exists(StartupSettings.StartupSettingsFilePath))
            {
                Debug.WriteLine($"[Monitor Service] No startup settings file found. Writing default startup settings to {StartupSettings.StartupSettingsFilePath}");
                startupSettings.WriteStartupSettingsFile();
            }
            //If there is a startup settings file, read it
            else
            {
                Debug.WriteLine($"[Monitor Service] Reading startup settings from {StartupSettings.StartupSettingsFilePath}");
                startupSettings.ReadStartupSettingsFile();
            }

            return startupSettings;
        }

        public static Settings InitSettings(StartupSettings startupSettings)
        {
            Settings settings = new Settings();

            //If there isn't a settings file, make one
            if (!File.Exists(startupSettings.SettingsFilePath))
            {
                Debug.WriteLine($"[Monitor Service] No settings file found. Writing default startup settings to {startupSettings.SettingsFilePath}");
                settings.WriteSettingsFile(startupSettings.SettingsFilePath);
            }
            //If there is a settings file, read it
            else
            {
                Debug.WriteLine($"[Monitor Service] Reading settings from {startupSettings.SettingsFilePath}");
                settings.ReadSettingsFile(startupSettings.SettingsFilePath);
            }

            return settings;
        }

        public static void UpdateBrightness(Settings settings)
        {
            //Console.Clear();
            //Console.WriteLine("\nUpdating Brightness...");

            currentTime = DateTime.Now;

            //When to start lowering brightness
            dimStartTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, settings.DimStartHour, settings.DimStartMinute, 0);
            //When to stop lowering brightness
            dimEndTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, settings.DimEndHour, settings.DimEndMinute, 0);
            //When to start raising brightness
            brightStartTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, settings.BrightStartHour, settings.BrightStartMinute, 0);
            //When to stop raising brightness
            brightEndTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, settings.BrightEndHour, settings.BrightEndMinute, 0);

            float percentBright = GetBrightnessPercentage();

            SetAllMonitorsBrightness(settings, percentBright);
            SetBlueLightFilter(settings, percentBright);

            //Console.Clear();
            //Console.WriteLine($"\nThe current time is {currentTime}");
            //Console.WriteLine($"Brightness: {Math.Round(percentBright * 100, 2)}%");
            //Console.WriteLine($"Brightness is updated every {settings.BrightCheckTime} seconds.");
        }

        public static void SetBlueLightFilter(Settings settings, float percentBrightness)
        {
            byte blueLightValue;

            blueLightValue = (byte)Math.Round(MapPercentage(settings.MaxBlueLightFilter, settings.MinBlueLightFilter, percentBrightness));

            SetMonitorValue(settings.MonitorCenter, VCPCodes.blueLightFilter, blueLightValue.ToString());
        }

        public static void SetAllMonitorsBrightness(Settings settings,float percentBrightness)
        {
            byte centerBrightness;
            byte leftBrightness;
            byte rightBrightness;

            centerBrightness = (byte)Math.Round(MapPercentage(settings.CenterMinBrightness, settings.CenterMaxBrightness, percentBrightness));
            leftBrightness = (byte)Math.Round(MapPercentage(settings.LeftMinBrightness, settings.LeftMaxBrightness, percentBrightness));
            rightBrightness = (byte)Math.Round(MapPercentage(settings.RightMinBrightness, settings.RightMaxBrightness, percentBrightness));

            //Set each monitor's brightness
            SetMonitorValue(settings.MonitorCenter, VCPCodes.brightness, centerBrightness.ToString());
            SetMonitorValue(settings.MonitorLeft, VCPCodes.brightness, leftBrightness.ToString());
            SetMonitorValue(settings.MonitorRight, VCPCodes.brightness, rightBrightness.ToString());
        }

        public static float GetBrightnessPercentage()
        {
            if (currentTime < brightStartTime || currentTime >= dimEndTime)
            {
                //We are at minimum brightness
                return 0;
            }

            if (currentTime >= brightEndTime && currentTime < dimStartTime)
            {
                //We are at maximum brightness
                return 1;
            }

            if (currentTime < brightEndTime)
            {
                //We are in the brightening stage
                return PercentageOfTime(currentTime, brightStartTime, brightEndTime);
            }
            else
            {
                //We are in the dimming stage
                return 1 - PercentageOfTime(currentTime, dimStartTime, dimEndTime);
            }
        }

        public static float PercentageOfTime(DateTime time, DateTime startTime, DateTime endTime)
        {
            float timeRange = (float)(endTime - startTime).TotalMinutes;

            float elapsedTime = (float)(time - startTime).TotalMinutes;

            float percentage = elapsedTime / timeRange;

            percentage = Math.Max(0, Math.Min(1, percentage));

            return percentage;
        }

        public static double MapPercentage(double minValue, double maxValue, double percentage)
        {
            //Short circuit for min and max percentage
            if (percentage <= 0)
            {
                return minValue;
            }

            if (percentage >= 1)
            {
                return maxValue;
            }

            return minValue + (maxValue - minValue) * percentage;
        }

        public static void SetMonitorValue(string monitorID, string vcpCode, string value)
        {
            Debug.WriteLine($"[Monitor Service] Setting VCP code \"{vcpCode}\" to {value} for monitor {monitorID}...");

            // Create process start info
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                FileName = Programs.controlMyMonitor,
                Arguments = $"/SetValueIfNeeded {monitorID} {vcpCode} {value}",
                UseShellExecute = false, // Don't use the system shell to execute the command
            };

            // Create and start the process
            Process process = new Process
            {
                StartInfo = processStartInfo
            };
            process.Start();
            process.WaitForExit();
            Debug.WriteLine("Completed!");
        }
    }
}
