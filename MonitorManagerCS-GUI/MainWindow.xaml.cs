using MonitorManagerCS_GUI.Core;
using MonitorManagerCS_GUI.ViewModels;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace MonitorManagerCS_GUI
{
    public partial class MainWindow : Window
    {
        private static readonly Uri _iconUri = new("pack://application:,,,/Assets/icon.ico",
            UriKind.Absolute);
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _trayMenuStrip;
        private ToolStripMenuItem _showTrayMenuItem;
        private ToolStripMenuItem _exitTrayMenuItem;
        public MainViewModel ViewModel { get; set; }
        private static readonly string[] _statusPrefixes = ["", "  ", "    "];
        private int _statusPrefixIndex;

        public MainWindow()
        {
            Settings.Load();

            //Load the XAML code
            InitializeComponent();

            InitializeTrayIcon();

            StateChanged += MainWindow_StateChanged;

            //#if !DEBUG
            if (Settings.StartInTray)
            {
                MinimizeToTray();
            }
            //#endif

            ViewModel = new MainViewModel();
            DataContext = ViewModel;
        }

        private void InitializeTrayIcon()
        {
            var _resourceInfo = System.Windows.Application.GetResourceStream(_iconUri);

            if (_resourceInfo != null)
            {
                using Stream iconStream = _resourceInfo.Stream;
                _trayIcon = new NotifyIcon
                {
                    Icon = new Icon(iconStream),
                    Visible = false,
                    Text = "Nathan's Monitor Manager"
                };
            }

            _trayMenuStrip = new ContextMenuStrip();
            _showTrayMenuItem = new ToolStripMenuItem("Show");
            _exitTrayMenuItem = new ToolStripMenuItem("Exit");

            _showTrayMenuItem.Click += ShowTrayMenuItem_Click;
            _exitTrayMenuItem.Click += ExitTrayMenuItem_Click;

            _trayMenuStrip.Items.Add(_showTrayMenuItem);
            _trayMenuStrip.Items.Add(_exitTrayMenuItem);

            _trayIcon.ContextMenuStrip = _trayMenuStrip;

            _trayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    Show();
                    WindowState = WindowState.Normal;
                    _trayIcon.Visible = false;
                }
            };
        }

        private void ShowTrayMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            _trayIcon.Visible = false;
        }

        private void ExitTrayMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && Settings.MinimizeToTray)
            {
                MinimizeToTray();
            }
        }

        private void MinimizeToTray()
        {
            Hide();
            _trayIcon.Visible = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _trayIcon.Dispose();
            ViewModel.EndService();
            System.Windows.Application.Current.Shutdown();
        }

        private string StatusPrefix()
        {
            string output = _statusPrefixes[_statusPrefixIndex++];
            if (_statusPrefixIndex >= _statusPrefixes.Length) _statusPrefixIndex = 0;
            return output;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            string exitQuestion = "Are you sure you want to exit?" + Environment.NewLine +
                Environment.NewLine + "This will exit the monitor service and disable automation.";

            var result = System.Windows.MessageBox.Show(exitQuestion, Title,
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        private void ToggleMaximized()
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                MainDockPanel.Margin = SystemParameters.WindowResizeBorderThickness;
            }
            else
            {
                WindowState = WindowState.Normal;
                MainDockPanel.Margin = new Thickness(0);
            }
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximized();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximized();
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    DragMove();
                }
                catch (InvalidOperationException)
                {
                    // DragMove can throw if mouse capture is lost - safe to ignore
                }
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        enum DwmWindowCornerPreference
        {
            Default = 0,
            DoNotRound = 1,
            Round = 2,
            RoundSmall = 3
        }

        [LibraryImport("dwmapi.dll", SetLastError = true)]
        internal static partial int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            //Let Windows 11 know to round the corners on this window
            var hwnd = new WindowInteropHelper(this).Handle;
            int attrValue = (int)DwmWindowCornerPreference.Round;
            DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref attrValue, sizeof(int));
        }
    }
}
