using CommunityToolkit.Mvvm.Input;
using MonitorManagerCS_GUI.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MonitorManagerCS_GUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TabViewModel> Tabs { get; set; }
        public int SelectedTabIndex { get; set; }
        private readonly DisplayManager _displayManager = new DisplayManager();

        public MainViewModel()
        {
            var Tab_DisplayTest = new DisplayTab
            {
                TabName = "Display Test"
            };

            var Tab_Settings = new SettingsTab
            {
                TabName = "Settings",
                Text = "This is a settings tab."
            };

            Tabs = new ObservableCollection<TabViewModel>
            {
                Tab_DisplayTest,
                Tab_Settings
            };

            SelectedTabIndex = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private RelayCommand getDisplaysCommand;
        public ICommand GetDisplaysCommand
        {
            get
            {
                if (getDisplaysCommand == null)
                {
                    getDisplaysCommand = new RelayCommand(_displayManager.GetDisplays);
                }

                return getDisplaysCommand;
            }
        }
    }
}
