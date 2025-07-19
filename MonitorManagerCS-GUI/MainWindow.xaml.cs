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
        public StartupSettings startupSettings;
        public Settings settings;
        public Task monitorServiceTask;
        private CancellationTokenSource monitorServiceTokenSource;
        private static readonly string[] statusPrefixes = { "", "  ", "    " };
        private byte statusPrefixIndex;

        /// <summary>
        /// Entry point of the program.
        /// </summary>
        public MainWindow()
        {
            //Mandatory line to make the main window work
            InitializeComponent();

            InitializeTrayIcon();

            //Run the MainWindow_StateChanged function when the window's state changes (e.g. when the window is opened, minimized, or closed)
            StateChanged += MainWindow_StateChanged;

            //MinimizeToTray();

            ViewModel = new MainViewModel();
            DataContext = ViewModel;
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
            public ObservableCollection<TabViewModel> Tabs { get; set; }

            public MainViewModel()
            {
                Tabs = new ObservableCollection<TabViewModel>
                {
                    new TabViewModel
                    {
                        TabName = "Tab 1"
                    }
                };
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class TabViewModel
        {
            public string TabName { get; set; } 
        }

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

            trayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    Show();
                    WindowState = WindowState.Normal;
                    trayIcon.Visible = false;
                }
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
                MinimizeToTray();
            }
        }

        private void MinimizeToTray()
        {
            Hide();
            trayIcon.Visible = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            trayIcon.Dispose();
        }

        public string GetSettingsPathFromUser()
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
                    System.Windows.MessageBox.Show($"The file path '{StartupSettings.StartupSettingsFilePath}' is reserved for startup settings.", "Invalid Filename", MessageBoxButton.OK, MessageBoxImage.Warning);
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
