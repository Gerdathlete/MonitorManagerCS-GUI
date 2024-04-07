using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Security.AccessControl;

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
            //Mandatory line to make the main window work
            InitializeComponent();

            //These two lines make it so that data bindings work, IDK how it works
            //Initialize the view model (idk how this works, ChatGPT did this)
            ViewModel = new MainViewModel();
            //Make the view model the data context (idk how this works, ChatGPT did this)
            DataContext = ViewModel;

            //Initialize the tray icon
            InitializeTrayIcon();
            //Run the MainWindow_StateChanged function when the window's state changes (this is used to make the minimize button minimize to tray)
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

            //Clear the settings data grid
            ViewModel.SettingsGridData.Clear();

            //Add all the settings to the settings data grid
            settingDimStart = new SettingsGridItem("When to start decreasing brightness", DataFormatter.GetReadableTime(DefaultSettings.DimStartHour, DefaultSettings.DimStartMinute), DataFormatter.GetReadableTime(settings.DimStartHour, settings.DimStartMinute));
            ViewModel.SettingsGridData.Add(settingDimStart);
            settingDimEnd = new SettingsGridItem("When monitors should reach minimum brightness", DataFormatter.GetReadableTime(DefaultSettings.DimEndHour, DefaultSettings.DimEndMinute), DataFormatter.GetReadableTime(settings.DimEndHour, settings.DimEndMinute));
            ViewModel.SettingsGridData.Add(settingDimEnd);
            settingBrightStart = new SettingsGridItem("When to start increasing brightness", DataFormatter.GetReadableTime(DefaultSettings.BrightStartHour, DefaultSettings.BrightStartMinute), DataFormatter.GetReadableTime(settings.BrightStartHour, settings.BrightStartMinute));
            ViewModel.SettingsGridData.Add(settingBrightStart);
            settingBrightEnd = new SettingsGridItem("When monitors should reach maximum brightness", DataFormatter.GetReadableTime(DefaultSettings.BrightEndHour, DefaultSettings.BrightEndMinute), DataFormatter.GetReadableTime(settings.BrightEndHour, settings.BrightEndMinute));
            ViewModel.SettingsGridData.Add(settingBrightEnd);
            settingMonitorLeft = new SettingsGridItem("Left monitor name or ID", DefaultSettings.MonitorLeft, settings.MonitorLeft);
            ViewModel.SettingsGridData.Add(settingMonitorLeft);
            settingMonitorCenter = new SettingsGridItem("Center monitor name or ID", DefaultSettings.MonitorCenter, settings.MonitorCenter);
            ViewModel.SettingsGridData.Add(settingMonitorCenter);
            settingMonitorRight = new SettingsGridItem("Right monitor name or ID", DefaultSettings.MonitorRight, settings.MonitorRight);
            ViewModel.SettingsGridData.Add(settingMonitorRight);
            settingLeftBrightness = new SettingsGridItem("Left monitor brightness range", DataFormatter.GetMinMaxString(DefaultSettings.LeftMinBrightness, DefaultSettings.LeftMaxBrightness), DataFormatter.GetMinMaxString(settings.LeftMinBrightness, settings.LeftMaxBrightness));
            ViewModel.SettingsGridData.Add(settingLeftBrightness);
            settingCenterBrightness = new SettingsGridItem("Center monitor brightness range", DataFormatter.GetMinMaxString(DefaultSettings.CenterMinBrightness, DefaultSettings.CenterMaxBrightness), DataFormatter.GetMinMaxString(settings.CenterMinBrightness, settings.CenterMaxBrightness));
            ViewModel.SettingsGridData.Add(settingCenterBrightness);
            settingRightBrightness = new SettingsGridItem("Right monitor brightness range", DataFormatter.GetMinMaxString(DefaultSettings.RightMinBrightness, DefaultSettings.RightMaxBrightness), DataFormatter.GetMinMaxString(settings.RightMinBrightness, settings.RightMaxBrightness));
            ViewModel.SettingsGridData.Add(settingRightBrightness);
            settingCenterBlueFilter = new SettingsGridItem("Center monitor blue light filter range", DataFormatter.GetMinMaxString(DefaultSettings.MinBlueLightFilter, DefaultSettings.MaxBlueLightFilter), DataFormatter.GetMinMaxString(settings.MinBlueLightFilter, settings.MaxBlueLightFilter));
            ViewModel.SettingsGridData.Add(settingCenterBlueFilter);
            settingBrightCheckTime = new SettingsGridItem("Time between brightness updates (seconds)", DefaultSettings.BrightCheckTime.ToString(), DefaultSettings.BrightCheckTime.ToString());
            ViewModel.SettingsGridData.Add(settingBrightCheckTime);
        }

        private void StartMonitorService()
        {
            //End the monitor service (if its running)
            EndMonitorService();

            //Create a new token source for the service (this allows it to be ended)
            monitorServiceTokenSource = new CancellationTokenSource();

            //Launch the monitor service
            Debug.WriteLine("Launching monitor service...");
            monitorServiceTask = Task.Run(() => { App.MonitorService(settings, monitorServiceTokenSource.Token); });
        }

        private void EndMonitorService()
        {
            if (monitorServiceTask != null && !monitorServiceTask.IsCompleted)
            {
                monitorServiceTokenSource.Cancel();
                Debug.WriteLine("Shutting down monitor service...");
                monitorServiceTask.Wait();
            }
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

        private string StatusPrefix()
        {
            string output = statusPrefixes[statusPrefixIndex++];
            if (statusPrefixIndex >= statusPrefixes.Length) statusPrefixIndex = 0;
            return output;
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
            CheckSettingRegex(DataInterpreter.readableTimeRegex, settingDimStart.CurrentVal, "Invalid time");
            CheckSettingRegex(DataInterpreter.readableTimeRegex, settingDimEnd.CurrentVal, "Invalid time");
            CheckSettingRegex(DataInterpreter.readableTimeRegex, settingBrightStart.CurrentVal, "Invalid time");
            CheckSettingRegex(DataInterpreter.readableTimeRegex, settingBrightEnd.CurrentVal, "Invalid time");
            CheckSettingRegex(DataInterpreter.minMaxRegex, settingLeftBrightness.CurrentVal, "Invalid brightness range");
            CheckSettingRegex(DataInterpreter.minMaxRegex, settingCenterBrightness.CurrentVal, "Invalid brightness range");
            CheckSettingRegex(DataInterpreter.minMaxRegex, settingRightBrightness.CurrentVal, "Invalid brightness range");
            CheckSettingRegex(DataInterpreter.blueLightRegex, settingCenterBlueFilter.CurrentVal, "Invalid blue light filter range");

            if (!int.TryParse(settingBrightCheckTime.CurrentVal, out int brightCheckTime))
            {
                InvalidSetting("Invalid time between brightness updates", settingBrightCheckTime.CurrentVal);
            }

            //If any of the settings are invalid, show an error message and exit early
            if (invalidSettings)
            {
                //If the settings are invalid, show an error message to the user
                System.Windows.MessageBox.Show($"{invalidList}", "Invalid Settings", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //If the settings are valid...

            //Convert the time into the hour and minute bytes and store them in the settings
            byte[] twoBytes = DataInterpreter.ParseReadableTime(settingDimStart.CurrentVal);
            settings.DimStartHour = twoBytes[0];
            settings.DimStartMinute = twoBytes[1];

            twoBytes = DataInterpreter.ParseReadableTime(settingDimEnd.CurrentVal);
            settings.DimEndHour = twoBytes[0];
            settings.DimEndMinute = twoBytes[1];

            twoBytes = DataInterpreter.ParseReadableTime(settingBrightStart.CurrentVal);
            settings.BrightStartHour = twoBytes[0];
            settings.BrightStartMinute = twoBytes[1];

            twoBytes = DataInterpreter.ParseReadableTime(settingBrightEnd.CurrentVal);
            settings.BrightEndHour = twoBytes[0];
            settings.BrightEndMinute = twoBytes[1];

            //Store monitor ids in settings
            settings.MonitorLeft = settingMonitorLeft.CurrentVal;
            settings.MonitorCenter = settingMonitorCenter.CurrentVal;
            settings.MonitorRight = settingMonitorRight.CurrentVal;

            //Store min and max brightnesses in the settings
            twoBytes = DataInterpreter.ParseMinMaxString(settingLeftBrightness.CurrentVal);
            settings.LeftMinBrightness = twoBytes[0];
            settings.LeftMaxBrightness = twoBytes[1];

            twoBytes = DataInterpreter.ParseMinMaxString(settingCenterBrightness.CurrentVal);
            settings.CenterMinBrightness = twoBytes[0];
            settings.CenterMaxBrightness = twoBytes[1];

            twoBytes = DataInterpreter.ParseMinMaxString(settingRightBrightness.CurrentVal);
            settings.RightMinBrightness = twoBytes[0];
            settings.RightMaxBrightness = twoBytes[1];

            //Store blue light filter range in the settings
            twoBytes = DataInterpreter.ParseMinMaxString(settingCenterBlueFilter.CurrentVal);
            settings.MinBlueLightFilter = twoBytes[0];
            settings.MaxBlueLightFilter = twoBytes[1];

            //Store the brightness check time in the settings
            settings.BrightCheckTime = brightCheckTime;

            //Save the settings file
            settings.WriteSettingsFile(startupSettings.SettingsFilePath);

            //Show a status message to let the user know it worked
            TxtStatus.Text = $"{StatusPrefix()} Settings saved to {startupSettings.SettingsFilePath}";
            //Restart the monitor manager service
            StartMonitorService();
        }

        private void BtnLoadSettings_Click(object sender, RoutedEventArgs e)
        {
            //If the settings path doesn't lead to a file, show an error message
            if (!File.Exists(TxtSettingsPath.Text))
            {
                System.Windows.MessageBox.Show($"File does not exist: {TxtSettingsPath.Text}", "Invalid file path",MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Ask the user if they want to continue
            MessageBoxResult msgBoxResult = System.Windows.MessageBox.Show($"Are you sure you want to change the settings file path to {TxtSettingsPath.Text}?", "Change settings file path?", MessageBoxButton.YesNo);
            if (msgBoxResult == MessageBoxResult.No)
            {
                return;
            }

            //Read the text out of the file and store it in a string
            string settingsText = File.ReadAllText(TxtSettingsPath.Text);

            //Attempt to read the settings file
            try
            {
                JsonSerializer.Deserialize<Settings>(settingsText);
            }
            //If the json reader can't read the settings file, show an error message
            catch (Exception)
            {
                System.Windows.MessageBox.Show($"Failed to read file\n {TxtSettingsPath.Text}", "Invalid file format", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Read the settings from the settings path
            settings.ReadSettingsFile(TxtSettingsPath.Text);
            //Update the data grid with the new settings
            PopulateSettingsGrid(settings);
            //Update the startup settings to the new path
            startupSettings.SettingsFilePath = TxtSettingsPath.Text;
            //Update the startup settings file
            startupSettings.WriteStartupSettingsFile();

            //Show a status message to let the user know it worked
            TxtStatus.Text = $"{StatusPrefix()}Loaded settings from {TxtSettingsPath.Text}";
            //Restart the monitor service to use the new settings
            StartMonitorService();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            EndMonitorService();
        }
    }

    public static class DataFormatter
    {
        public static string GetReadableTime(byte hour, byte minute)
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

        public static string GetMinMaxString(byte min, byte max)
        {
            return $"{min}-{max}";
        }
    }

    public static class DataInterpreter
    {
        public static byte[] ParseReadableTime(string time)
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

        public static Regex blueLightRegex = new Regex(@"^[0-4]-[0-4]$");

        public static Regex minMaxRegex = new Regex(@"^(?:0|[1-9][0-9]?|100)-(?:0|[1-9][0-9]?|100)$");

        /// <summary>
        /// Turns a min-max string into two bytes:
        /// "&lt;min&gt;-&lt;max&gt;" -&gt; byte {min, max}
        /// </summary>
        /// <param name="minMaxString"></param>
        /// <returns></returns>
        public static byte[] ParseMinMaxString(string minMaxString)
        {
            //Example input: 15-100
            string[] minMax = minMaxString.Split('-');
            return new byte[] { byte.Parse(minMax[0]), byte.Parse(minMax[1]) };

        }
    }
}
