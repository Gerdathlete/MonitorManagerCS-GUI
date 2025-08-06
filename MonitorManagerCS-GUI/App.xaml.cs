using MonitorManagerCS_GUI.Core;
using System;
using System.Windows;

namespace MonitorManagerCS_GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (!Folders.CanReadAndWrite(out Exception ex))
            {
                MessageBox.Show($"The application does not have access to required directories." +
                    Environment.NewLine + Environment.NewLine + $" {ex}",
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}
