using Newtonsoft.Json;
using System.IO;

namespace MonitorManagerCS_GUI.Core
{
    public static class Settings
    {
        public static bool StartInTray { get; set; } = true;
        public static bool MinimizeToTray { get; set; } = true;

        private class SettingsData
        {
            public bool StartInTray { get; set; }
            public bool MinimizeToTray { get; set; }
        }

        private const string _fileName = "settings.json";

        public static void Save()
        {
            var fileDirectory = Folders.Config;
            var filePath = Path.Combine(fileDirectory, _fileName);

            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);

            var settingsData = new SettingsData
            {
                StartInTray = StartInTray,
                MinimizeToTray = MinimizeToTray
            };

            string json = JsonConvert.SerializeObject(settingsData, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static void Load()
        {
            var filePath = Path.Combine(Folders.Config, _fileName);

            if (!File.Exists(filePath))
                return;

            string json = File.ReadAllText(filePath);
            var settingsData = JsonConvert.DeserializeObject<SettingsData>(json);

            if (settingsData != null)
            {
                StartInTray = settingsData.StartInTray;
                MinimizeToTray = settingsData.MinimizeToTray;
            }
        }
    }
}
