using CommunityToolkit.Mvvm.Input;
using MonitorManagerCS_GUI.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

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
        private TabViewModel _selectedTab;
        public TabViewModel SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    OnPropertyChanged(nameof(SelectedTab));
                }
            }
        }
        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    OnPropertyChanged(nameof(SelectedTabIndex));
                }
            }
        }
        private readonly DisplayManager _displayManager = new DisplayManager();

        public MainViewModel()
        {
            var tab_Settings = new SettingsTab
            {
                TabName = "Settings",
                Text = "This is a settings tab."
            };

            var defaultTabs = new List<TabViewModel>()
            {
                tab_Settings
            };

            Tabs = new ObservableCollection<TabViewModel>(defaultTabs);

            SelectedTabIndex = 0;
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

        public async void UpdateDisplayTabs()
        {
            var displays = await _displayManager.GetDisplays();

            await RemoveDisplayTabs();

            foreach (var display in displays)
            {
                var displayTab = new DisplayTab(display);

                Tabs.Insert(0, displayTab);

                //Select brightness by default
                displayTab.SelectVCPCode("10");
            }

            SelectedTabIndex = 0;
        }

        private async Task RemoveDisplayTabs()
        {
            var displayTabs = Tabs.OfType<DisplayTab>().ToList();

            if (SelectedTab is DisplayTab)
            {
                SelectedTab = null;
            }

            //Remove the tabs after WPF finishes doing things (this prevents binding errors)
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                foreach (var tab in displayTabs)
                {
                    Tabs.Remove(tab);
                }
            }, DispatcherPriority.Background);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
