using MonitorManagerCS_GUI.ViewModels;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace MonitorManagerCS_GUI
{
    public partial class MainWindow : Window
    {
        private static readonly Uri _iconUri = new Uri("pack://application:,,,/Resources/icon.ico", 
            UriKind.Absolute);
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _trayMenuStrip;
        private ToolStripMenuItem _showTrayMenuItem;
        private ToolStripMenuItem _exitTrayMenuItem;
        public MainViewModel ViewModel { get; set; }
        private static readonly string[] _statusPrefixes = { "", "  ", "    " };
        private int _statusPrefixIndex;

        public MainWindow()
        {
            //Mandatory line to load the main window
            InitializeComponent();

            InitializeTrayIcon();

            StateChanged += MainWindow_StateChanged;

#if !DEBUG
            MinimizeToTray();
#endif

            ViewModel = new MainViewModel();
            DataContext = ViewModel;
        }

        private void InitializeTrayIcon()
        {
            var _resourceInfo = System.Windows.Application.GetResourceStream(_iconUri);

            if (_resourceInfo != null)
            {
                using (Stream iconStream = _resourceInfo.Stream)
                {
                    _trayIcon = new NotifyIcon
                    {
                        Icon = new Icon(iconStream),
                        Visible = false,
                        Text = "Nathan's Monitor Manager"
                    };
                }
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
            if (WindowState == WindowState.Minimized)
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
        }

        private string StatusPrefix()
        {
            string output = _statusPrefixes[_statusPrefixIndex++];
            if (_statusPrefixIndex >= _statusPrefixes.Length) _statusPrefixIndex = 0;
            return output;
        }
    }
}
