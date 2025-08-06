using CommunityToolkit.Mvvm.Input;
using MonitorManagerCS_GUI.Core;
using System;
using System.Windows.Input;

namespace MonitorManagerCS_GUI.ViewModels
{
    public class SettingsTab : TabViewModel
    {
        public bool StartInTray
        {
            get => Settings.StartInTray;
            set
            {
                if (Settings.StartInTray != value)
                {
                    Settings.StartInTray = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool MinimizeToTray
        {
            get => Settings.MinimizeToTray;
            set
            {
                if (Settings.MinimizeToTray != value)
                {
                    Settings.MinimizeToTray = value;
                    OnPropertyChanged();
                }
            }
        }

        public SettingsTab()
        {
            TabName = "Settings";
        }

        private RelayCommand saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                {
                    saveCommand = new RelayCommand(Save);
                }

                return saveCommand;
            }
        }
        public void Save() => Settings.Save();

        private RelayCommand exitButtonCommand;
        public ICommand ExitButtonCommand
        {
            get
            {
                if (exitButtonCommand == null)
                {
                    exitButtonCommand = new RelayCommand(ExitButton);
                }

                return exitButtonCommand;
            }
        }
        public event EventHandler ExitButtonPressed;
        private void ExitButton()
        {
            ExitButtonPressed?.Invoke(this, EventArgs.Empty);
        }
    }
}
