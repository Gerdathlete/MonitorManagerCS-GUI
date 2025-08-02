using CommunityToolkit.Mvvm.Input;
using MonitorManagerCS_GUI.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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

        private SettingsTab _settingsTab;
        private DisplaysTab _displaysTab;

        public MainViewModel()
        {
            //Skip the rest of the constructor to prevent running the monitor service
            //in the design window
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            _monitorService = MonitorService.Instance();

            Init();
        }

        public async void Init()
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

            SelectedTabIndex = 0;

            var displayManagers = await LoadDisplaysAsync();

            _monitorService.DisplayManagers = displayManagers;
            _monitorService.UpdatePeriodMillis = 5 * 1000;

            _monitorService.Start();
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
        public async void LoadDisplays()
        {
            _monitorService.DisplayManagers = await LoadDisplaysAsync();
        }

        public async Task<List<DisplayManager>> LoadDisplaysAsync()
        {
            var displays = await DisplayRetriever.GetDisplayList();

            _displaysTab.Displays = displays;

            RemoveDisplayTabs();

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

                var displayTab = new DisplayTab(displayManager);

                Tabs.Insert(0, displayTab);

                //Select brightness by default
                displayTab.SelectVCPCode("10");
            }

            SelectedTabIndex = 0;

            return displayManagers;
        }

        private void RemoveDisplayTabs()
        {
            var displayTabs = Tabs.OfType<DisplayTab>().ToList();

            foreach (var tab in displayTabs)
            {
                RemoveTab(tab);
            }
        }

        public void RemoveTab(TabViewModel tab)
        {
            BindingErrors.Hide();

            Tabs.Remove(tab);

            BindingErrors.Show();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
