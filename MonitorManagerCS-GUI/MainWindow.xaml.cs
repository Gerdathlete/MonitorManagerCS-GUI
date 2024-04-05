using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace MonitorManagerCS_GUI
{
    public partial class MainWindow : Window
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenuStrip;
        private ToolStripMenuItem showTrayMenuItem;
        private ToolStripMenuItem exitTrayMenuItem;
        public MainViewModel ViewModel { get; set; }
        public SettingsGridItem settingDimStart;
        public SettingsGridItem settingDimEnd;
        public SettingsGridItem settingBrightStart;
        public SettingsGridItem settingBrightEnd;
        public SettingsGridItem settingMonitorLeft;
        public SettingsGridItem settingMonitorCenter;
        public SettingsGridItem settingMonitorRight;
        public SettingsGridItem settingLeftBrightness;
        public SettingsGridItem settingCenterBrightness;
        public SettingsGridItem settingRightBrightness;
        public SettingsGridItem settingCenterBlueFilter;
        public SettingsGridItem settingBrightCheckTime;
        public StartupSettings startupSettings;
        public Settings settings;
        public Task monitorServiceTask;
        private CancellationTokenSource monitorServiceTokenSource;
        private static readonly string[] statusPrefixes = { "", "  ", "    " };
        private byte statusPrefixIndex;

        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainViewModel(); // Initialize your view model
            DataContext = ViewModel; // Set the DataContext to the view mode

            InitializeTrayIcon();
            StateChanged += MainWindow_StateChanged;

            //Load settings
            startupSettings = App.InitStartupSettings();
            settings = App.InitSettings(startupSettings);

            //Update the text in the settings file path text box
            TxtSettingsPath.Text = startupSettings.SettingsFilePath;

            //Run the monitor service
            StartMonitorService();

            //Add all the settings to the settings data grid
            PopulateSettingsGrid(settings);
        }

        public void PopulateSettingsGrid(Settings settings)
        {
            //Throw an exception if the settings don't exist
            if (settings == null) { throw new ArgumentNullException(nameof(settings), "Settings cannot be null"); }

            //Add all the settings to the settings data grid
            settingDimStart = new SettingsGridItem("When to start decreasing brightness", GetReadableTime(DefaultSettings.DimStartHour, DefaultSettings.DimStartMinute), GetReadableTime(settings.DimStartHour, settings.DimStartMinute));
            ViewModel.SettingsGridData.Add(settingDimStart);
            settingDimEnd = new SettingsGridItem("When monitors should reach minimum brightness", GetReadableTime(DefaultSettings.DimEndHour, DefaultSettings.DimEndMinute), GetReadableTime(settings.DimEndHour, settings.DimEndMinute));
            ViewModel.SettingsGridData.Add(settingDimEnd);
            settingBrightStart = new SettingsGridItem("When to start increasing brightness", GetReadableTime(DefaultSettings.BrightStartHour, DefaultSettings.BrightStartMinute), GetReadableTime(settings.BrightStartHour, settings.BrightStartMinute));
            ViewModel.SettingsGridData.Add(settingBrightStart);
            settingBrightEnd = new SettingsGridItem("When monitors should reach maximum brightness", GetReadableTime(DefaultSettings.BrightEndHour, DefaultSettings.BrightEndMinute), GetReadableTime(settings.BrightEndHour, settings.BrightEndMinute));
            ViewModel.SettingsGridData.Add(settingBrightEnd);
            settingMonitorLeft = new SettingsGridItem("Left monitor name or ID", DefaultSettings.MonitorLeft, settings.MonitorLeft);
            ViewModel.SettingsGridData.Add(settingMonitorLeft);
            settingMonitorCenter = new SettingsGridItem("Center monitor name or ID", DefaultSettings.MonitorCenter, settings.MonitorCenter);
            ViewModel.SettingsGridData.Add(settingMonitorCenter);
            settingMonitorRight = new SettingsGridItem("Right monitor name or ID", DefaultSettings.MonitorRight, settings.MonitorRight);
            ViewModel.SettingsGridData.Add(settingMonitorRight);
            settingLeftBrightness = new SettingsGridItem("Left monitor brightness range", GetMinMaxString(DefaultSettings.LeftMinBrightness, DefaultSettings.LeftMaxBrightness), GetMinMaxString(settings.LeftMinBrightness, settings.LeftMaxBrightness));
            ViewModel.SettingsGridData.Add(settingLeftBrightness);
            settingCenterBrightness = new SettingsGridItem("Center monitor brightness range", GetMinMaxString(DefaultSettings.CenterMinBrightness, DefaultSettings.CenterMaxBrightness), GetMinMaxString(settings.CenterMinBrightness, settings.CenterMaxBrightness));
            ViewModel.SettingsGridData.Add(settingCenterBrightness);
            settingRightBrightness = new SettingsGridItem("Right monitor brightness range", GetMinMaxString(DefaultSettings.RightMinBrightness, DefaultSettings.RightMaxBrightness), GetMinMaxString(settings.RightMinBrightness, settings.RightMaxBrightness));
            ViewModel.SettingsGridData.Add(settingRightBrightness);
            settingCenterBlueFilter = new SettingsGridItem("Center monitor blue light filter range", GetMinMaxString(DefaultSettings.MinBlueLightFilter, DefaultSettings.MaxBlueLightFilter), GetMinMaxString(settings.MinBlueLightFilter, settings.MaxBlueLightFilter));
            ViewModel.SettingsGridData.Add(settingCenterBlueFilter);
            settingBrightCheckTime = new SettingsGridItem("Time between brightness updates (seconds)", DefaultSettings.BrightCheckTime.ToString(), DefaultSettings.BrightCheckTime.ToString());
            ViewModel.SettingsGridData.Add(settingBrightCheckTime);
        }

        private void StartMonitorService()
        {
            if (monitorServiceTask != null && !monitorServiceTask.IsCompleted)
            {
                monitorServiceTokenSource.Cancel();
                Debug.WriteLine("Shutting down monitor service...");
                monitorServiceTask.Wait();
            }

            monitorServiceTokenSource = new CancellationTokenSource();

            Debug.WriteLine("Launching monitor service...");
            monitorServiceTask = Task.Run(() => { App.MonitorService(settings, monitorServiceTokenSource.Token); });
        }

        public class MainViewModel : INotifyPropertyChanged
        {
            private ObservableCollection<SettingsGridItem> _settingsGridData;

            public ObservableCollection<SettingsGridItem> SettingsGridData
            {
                get { return _settingsGridData; }
                set
                {
                    _settingsGridData = value;
                    OnPropertyChanged(nameof(SettingsGridData));
                }
            }

            public MainViewModel()
            {
                // Initialize SettingsGridData
                SettingsGridData = new ObservableCollection<SettingsGridItem>();
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class SettingsGridItem : INotifyPropertyChanged
        {
            private string _settingName;
            public string SettingName
            {
                get { return _settingName; }
                set
                {
                    _settingName = value;
                    OnPropertyChanged(nameof(SettingName));
                }
            }

            private string _defaultVal;
            public string DefaultVal
            {
                get { return _defaultVal; }
                set
                {
                    _defaultVal = value;
                    OnPropertyChanged(nameof(DefaultVal));
                }
            }

            private string _currentVal;
            public string CurrentVal
            {
                get { return _currentVal; }
                set
                {
                    _currentVal = value;
                    OnPropertyChanged(nameof(CurrentVal));
                }
            }

            public SettingsGridItem(string settingName, string defaultValue, string currentValue)
            {
                SettingName = settingName;
                DefaultVal = defaultValue;
                CurrentVal = currentValue;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ObservableCollection<SettingsGridItem> SettingsGridData { get; set; }

        private void InitializeTrayIcon()
        {
            trayIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon("icon.ico"),
                Visible = false,
                Text = "Nathan\'s Monitor Manager" // Tooltip text
            };

            trayMenuStrip = new ContextMenuStrip();
            showTrayMenuItem = new ToolStripMenuItem("Show");
            exitTrayMenuItem = new ToolStripMenuItem("Exit");

            showTrayMenuItem.Click += ShowTrayMenuItem_Click;
            exitTrayMenuItem.Click += ExitTrayMenuItem_Click;

            trayMenuStrip.Items.Add(showTrayMenuItem);
            trayMenuStrip.Items.Add(exitTrayMenuItem);

            trayIcon.ContextMenuStrip = trayMenuStrip;

            trayIcon.DoubleClick += (s, e) =>
            {
                Show();
                WindowState = WindowState.Normal;
                trayIcon.Visible = false;
            };
        }

        private void ShowTrayMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            trayIcon.Visible = false;
        }

        private void ExitTrayMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                trayIcon.Visible = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            trayIcon.Dispose();
        }

        private void BtnSettingsPath_Click(object sender, RoutedEventArgs e)
        {
            string filePath = GetFilePath();
            if (filePath == null) { return; }

            TxtSettingsPath.Text = filePath;
        }

        public string GetFilePath()
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Filter = "JSON Files|*.json",
                Title = "Select a Monitor Manager settings file",
                InitialDirectory = StartupSettings.DefaultSettingsFilePath
            };

            //Stop the user from selecting the startup settings file
            fileDialog.FileOk += (sender, e) =>
            {
                if (fileDialog.FileName == StartupSettings.StartupSettingsFilePath)
                {
                    e.Cancel = true;
                    System.Windows.MessageBox.Show($"The file path 'StartupSettings.StartupSettingsFilePath' is reserved for startup settings.", "Invalid Filename", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            DialogResult diaResult = fileDialog.ShowDialog();

            if (diaResult == System.Windows.Forms.DialogResult.OK)
            {
                return fileDialog.FileName;
            }
            Debug.WriteLine("User did not select a file.");
            return null;
        }

        public string GetReadableTime(byte hour, byte minute)
        {
            string minuteStr = "";
            string AMPM = "AM";

            //If the hour is greater than 12, convert to PM
            if (hour > 12)
            {
                //Subtract 12 from the time, Make it PM
                hour -= 12;
                AMPM = "PM";
            }

            //If the hour is zero, make it 12 (this is 12 AM)
            if (hour == 0)
            {
                hour = 12;
            }

            //If the minute is less than 10, add a leading zero
            if (minute < 10)
            {
                //Add a zero before the minute
                minuteStr = "0";
            }

            return $"{hour}:{minuteStr}{minute} {AMPM}";
        }

        public byte[] ParseReadableTime(string time)
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
            return new byte[] { hour, minute };
        }

        public static Regex readableTimeRegex = new Regex(@"^([1-9]|1[0-2]):[0-5][0-9] (AM|PM)$");

        private string StatusPrefix()
        {
            string output = statusPrefixes[statusPrefixIndex++];
            if (statusPrefixIndex >= statusPrefixes.Length) statusPrefixIndex = 0;
            return output;
        }

        public static Regex blueLightRegex = new Regex(@"^[0-4]-[0-4]$");

        public static Regex minMaxRegex = new Regex(@"^(?:0|[1-9][0-9]?|100)-(?:0|[1-9][0-9]?|100)$");

        public string GetMinMaxString(byte min, byte max)
        {
            return $"{min}-{max}";
        }

        /// <summary>
        /// Turns a min-max string into two bytes:
        /// "&lt;min&gt;-&lt;max&gt;" -&gt; byte {min, max}
        /// </summary>
        /// <param name="minMaxString"></param>
        /// <returns></returns>
        public byte[] ParseMinMaxString(string minMaxString)
        {
            //Example input: 15-100
            string[] minMax = minMaxString.Split('-');
            return new byte[] { byte.Parse(minMax[0]), byte.Parse(minMax[1]) };

        }

        private void BtnUpdateSettings_Click(object sender, RoutedEventArgs e)
        {
            //When you press the update settings button
            //Take each "current" setting from the grid list, put it into the settings object, and then write it to the settings file.
            bool invalidSettings = false;
            string invalidList = "";

            void InvalidSetting(string invalidText, string inputString) { invalidSettings = true; invalidList += $"{invalidText}: {inputString}\n"; }

            void CheckSettingRegex(Regex regex, string inputString, string invalidText)
            {
                if (!regex.IsMatch(inputString)) { InvalidSetting(invalidText, inputString); }
            }

            //Check if the inputted settings are valid
            CheckSettingRegex(readableTimeRegex, settingDimStart.CurrentVal, "Invalid time");
            CheckSettingRegex(readableTimeRegex, settingDimEnd.CurrentVal, "Invalid time");
            CheckSettingRegex(readableTimeRegex, settingBrightStart.CurrentVal, "Invalid time");
            CheckSettingRegex(readableTimeRegex, settingBrightEnd.CurrentVal, "Invalid time");
            CheckSettingRegex(minMaxRegex, settingLeftBrightness.CurrentVal, "Invalid brightness range");
            CheckSettingRegex(minMaxRegex, settingCenterBrightness.CurrentVal, "Invalid brightness range");
            CheckSettingRegex(minMaxRegex, settingRightBrightness.CurrentVal, "Invalid brightness range");
            CheckSettingRegex(blueLightRegex, settingCenterBlueFilter.CurrentVal, "Invalid blue light filter range");

            if (!int.TryParse(settingBrightCheckTime.CurrentVal, out int brightCheckTime))
            {
                InvalidSetting("Invalid time between brightness updates", settingBrightCheckTime.CurrentVal);
            }

            if (!invalidSettings)
            {
                //If the settings are valid...

                //Convert the time into the hour and minute bytes and store them in the settings
                byte[] twoBytes = ParseReadableTime(settingDimStart.CurrentVal);
                settings.DimStartHour = twoBytes[0];
                settings.DimStartMinute = twoBytes[1];

                twoBytes = ParseReadableTime(settingDimEnd.CurrentVal);
                settings.DimEndHour = twoBytes[0];
                settings.DimEndMinute = twoBytes[1];

                twoBytes = ParseReadableTime(settingBrightStart.CurrentVal);
                settings.BrightStartHour = twoBytes[0];
                settings.BrightStartMinute = twoBytes[1];

                twoBytes = ParseReadableTime(settingBrightEnd.CurrentVal);
                settings.BrightEndHour = twoBytes[0];
                settings.BrightEndMinute = twoBytes[1];

                //Store monitor ids in settings
                settings.MonitorLeft = settingMonitorLeft.CurrentVal;
                settings.MonitorCenter = settingMonitorCenter.CurrentVal;
                settings.MonitorRight = settingMonitorRight.CurrentVal;

                //Store min and max brightnesses in the settings
                twoBytes = ParseMinMaxString(settingLeftBrightness.CurrentVal);
                settings.LeftMinBrightness = twoBytes[0];
                settings.LeftMaxBrightness = twoBytes[1];

                twoBytes = ParseMinMaxString(settingCenterBrightness.CurrentVal);
                settings.CenterMinBrightness = twoBytes[0];
                settings.CenterMaxBrightness = twoBytes[1];

                twoBytes = ParseMinMaxString(settingRightBrightness.CurrentVal);
                settings.RightMinBrightness = twoBytes[0];
                settings.RightMaxBrightness = twoBytes[1];

                //Store blue light filter range in the settings
                twoBytes = ParseMinMaxString(settingCenterBlueFilter.CurrentVal);
                settings.MinBlueLightFilter = twoBytes[0];
                settings.MaxBlueLightFilter = twoBytes[1];

                //Store the brightness check time in the settings
                settings.BrightCheckTime = brightCheckTime;

                //Save the settings file
                settings.WriteSettingsFile(startupSettings.SettingsFilePath);

                TxtStatus.Text = $"{StatusPrefix()} Settings saved to {startupSettings.SettingsFilePath}";
                //Restart the monitor manager service
                StartMonitorService();
            }
            else
            {
                //If the settings are invalid, show an error message to the user
                System.Windows.MessageBox.Show($"{invalidList}", "Invalid Settings", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLoadSettings_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public static class DataValidation
    {

    }
}
