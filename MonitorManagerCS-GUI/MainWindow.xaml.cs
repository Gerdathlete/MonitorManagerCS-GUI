using MonitorManagerCS_GUI.ViewModels;
using System;
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
        private static readonly string[] statusPrefixes = { "", "  ", "    " };
        private byte statusPrefixIndex;
        private static readonly double initWidth = 800;
        private static readonly double initHeight = 450;

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

        private string StatusPrefix()
        {
            string output = statusPrefixes[statusPrefixIndex++];
            if (statusPrefixIndex >= statusPrefixes.Length) statusPrefixIndex = 0;
            return output;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            MinWidth = ActualWidth;
            MinHeight = ActualHeight;

            SizeToContent = SizeToContent.Manual;

            Width = initWidth;
            Height = initHeight;
        }
    }
}
