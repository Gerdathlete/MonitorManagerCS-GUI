using CommunityToolkit.Mvvm.Input;
using MonitorManagerCS_GUI.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MonitorManagerCS_GUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
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

        private readonly MonitorService _monitorService;
        private readonly SettingsTab _settingsTab;
        private readonly DisplaysTab _displaysTab;

        public MainViewModel()
        {
            _settingsTab = new SettingsTab
            {
                TabName = "Settings",
                Text = "This is a settings tab."
            };

            _displaysTab = new DisplaysTab();

            var defaultTabs = new List<TabViewModel>()
            {
                _displaysTab,
                _settingsTab,
            };

            Tabs = new ObservableCollection<TabViewModel>(defaultTabs);

            SelectedTab = _displaysTab;

            //Skip the rest of the constructor to prevent running the monitor service
            //in the design window
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            _monitorService = MonitorService.Instance();
            _displaysTab.MonitorService = _monitorService;

            _ = Task.Run(async () =>
            {
                await GetAndUpdateDisplays();

                _monitorService.UpdatePeriodMillis =
#if DEBUG
                5 * 1000; //Every five seconds
#else
                5 * 60 * 1000; //Every five minutes
#endif
                _monitorService.Start();
            });
        }

        private RelayCommand _startServiceCommand;
        public ICommand StartServiceCommand
        {
            get
            {
                if (_startServiceCommand == null)
                {
                    _startServiceCommand = new RelayCommand(StartService);
                }

                return _startServiceCommand;
            }
        }
        public void StartService()
        {
            _monitorService.Start();
        }

        private RelayCommand _endServiceCommand;
        public ICommand EndServiceCommand
        {
            get
            {
                if (_endServiceCommand == null)
                {
                    _endServiceCommand = new RelayCommand(EndService);
                }

                return _endServiceCommand;
            }
        }
        public void EndService()
        {
            _ = _monitorService.End();
        }

        private RelayCommand _restartServiceCommand;
        public ICommand RestartServiceCommand
        {
            get
            {
                if (_restartServiceCommand == null)
                {
                    _restartServiceCommand = new RelayCommand(RestartService);
                }

                return _restartServiceCommand;
            }
        }
        public void RestartService()
        {
            _ = _monitorService.Restart();
        }

        private RelayCommand _loadDisplaysCommand;
        public ICommand LoadDisplaysCommand
        {
            get
            {
                if (_loadDisplaysCommand == null)
                {
                    _loadDisplaysCommand = new RelayCommand(LoadDisplays);
                }

                return _loadDisplaysCommand;
            }
        }
        public void LoadDisplays()
        {
            Task.Run(GetAndUpdateDisplays);
        }

        private RelayCommand updateConfigsCommand;
        public ICommand UpdateConfigsCommand
        {
            get
            {
                if (updateConfigsCommand == null)
                {
                    updateConfigsCommand = new RelayCommand(UpdateConfigs);
                }

                return updateConfigsCommand;
            }
        }
        private void UpdateConfigs()
        {
            Task.Run(UpdateOldConfigs);
        }

        public async Task UpdateOldConfigs()
        {
            var displays = await DisplayRetriever.GetDisplayList();
            var displayManagers = new List<DisplayManager>();

            foreach (var display in displays)
            {
                var displayManager = await DisplayManager.ConvertOldConfig(display);
                displayManagers.Add(displayManager);
            }

            UpdateDisplays(displays, displayManagers);
        }

        public async Task<List<DisplayManager>> GetDisplayManagers(List<DisplayInfo> displays)
        {
            var displayManagers = new List<DisplayManager>();
            foreach (var display in displays)
            {
                var displayManager = DisplayManager.Load(display);

                if (displayManager == null)
                {
                    var vcpCodes = await DisplayRetriever.GetVCPCodes(display);
                    displayManager = new DisplayManager(display, vcpCodes);
                }

                displayManagers.Add(displayManager);
            }

            return displayManagers;
        }

        public async Task GetAndUpdateDisplays()
        {
            var displays = await DisplayRetriever.GetDisplayList();
            var displayManagers = await GetDisplayManagers(displays);

            UpdateDisplays(displays, displayManagers);
        }

        public void UpdateDisplays(List<DisplayInfo> displays, List<DisplayManager> displayManagers)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //This needs to run on the UI thread since it sets an ObservableCollection
                _displaysTab.Displays = displays;

                //This has to be run after displays is set
                _displaysTab.DisplayManagers = displayManagers;
            });

            _monitorService.DisplayManagers = displayManagers;
        }

        public void RemoveTab(TabViewModel tab)
        {
            BindingErrors.Hide();

            Tabs.Remove(tab);

            BindingErrors.Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
