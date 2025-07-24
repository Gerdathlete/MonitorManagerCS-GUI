using CommunityToolkit.Mvvm.Input;
using MonitorManagerCS_GUI.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Windows.Input;

namespace MonitorManagerCS_GUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<TabViewModel> _tabs;
        public ObservableCollection<TabViewModel> Tabs
        {
            get => _tabs;
            set
            {
                if (_tabs != value)
                {
                    _tabs = value;
                    OnPropertyChanged(nameof(Tabs));
                }
            }
        }
        public int SelectedTabIndex { get; set; }
        private readonly DisplayManager _displayManager = new DisplayManager();
        private List<TabViewModel> _staticTabs;

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

            _staticTabs = new List<TabViewModel>()
            {
                Tab_DisplayTest,
                Tab_Settings
            };

            Tabs = new ObservableCollection<TabViewModel>(_staticTabs);

            SelectedTabIndex = 0;
        }

        public void UpdateDisplayTabs()
        {
            _tabs.Clear();

            _displayManager.GetDisplays();
            var displays = _displayManager.Displays;
            foreach (var display in displays)
            {
                _tabs.Add(new DisplayTab()
                {
                    TabName = $"{display.ShortID} (SN: {display.SerialNumber})",
                });
            }

            AddStaticTabs();
        }

        private void AddStaticTabs()
        {
            foreach (var tab in _staticTabs)
            {
                _tabs.Add(tab);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private RelayCommand _updateDisplayTabsCommand;
        public ICommand UpdateDisplayTabsCommand
        {
            get
            {
                if (_updateDisplayTabsCommand == null)
                {
                    _updateDisplayTabsCommand = new RelayCommand(UpdateDisplayTabs);
                }

                return _updateDisplayTabsCommand;
            }
        }
    }
}
