using MonitorManagerCS_GUI.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace MonitorManagerCS_GUI.ViewModels
{
    public class DisplaysTab : TabViewModel
    {
        private ObservableCollection<TabViewModel> _tabs = new ObservableCollection<TabViewModel>();
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

        private List<DisplayInfo> _displays;
        public List<DisplayInfo> Displays
        {
            get { return _displays; }
            set
            {
                if (_displays != value)
                {
                    _displays = value;
                    _selectorTab.DisplayViewModels = GetVMsFromDisplays(value);
                }
            }
        }

        private List<DisplayManager> _displayManagers;
        public List<DisplayManager> DisplayManagers
        {
            get => _displayManagers;
            set
            {
                if (_displayManagers != value)
                {
                    _displayManagers = value;
                    UpdateDisplayTabs();
                }
            }
        }

        private readonly DisplaySelector _selectorTab = new DisplaySelector();

        public double Scale { get; set; } = 0.1;
        internal MonitorService MonitorService { get; set; }

        public DisplaysTab()
        {
            TabName = "Displays";

            Tabs.Add(_selectorTab);
            SelectedTab = _selectorTab;
        }

        private void UpdateDisplayTabs()
        {
            RemoveDisplayTabs();

            foreach (var displayManager in _displayManagers)
            {
                var displayTab = new DisplayTab(displayManager, this);

                Tabs.Add(displayTab);

                //Select brightness by default
                displayTab.SelectVCPCode("10");
                displayTab.ExitButtonPressed += DisplayTab_ExitButtonPressed;
            }

            SelectedTab = _selectorTab;
        }

        private void DisplayTab_ExitButtonPressed(object sender, EventArgs e)
        {
            SelectedTab = _selectorTab;
        }

        public void RemoveDisplayTabs()
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

        private ObservableCollection<DisplayViewModel> GetVMsFromDisplays(List<DisplayInfo> displays)
        {
            return new ObservableCollection<DisplayViewModel>(
                displays.Select(d =>
                {
                    var vm = new DisplayViewModel(d, Scale);
                    vm.LeftClicked += DisplayVM_OnLeftClicked;
                    return vm;
                }));
        }

        private void DisplayVM_OnLeftClicked(object sender, DisplayInfo display)
        {
            var displayTab = GetDisplayTab(display);

            if (displayTab is null) return;

            SelectedTab = displayTab;
        }

        private DisplayTab GetDisplayTab(DisplayInfo display)
        {
            var displayTabs = Tabs
                .Where(t => t is DisplayTab)
                .Select(t => (DisplayTab)t);

            foreach (var displayTab in displayTabs)
            {
                if (displayTab.DisplayManager.Display == display)
                {
                    return displayTab;
                }
            }

            return null;
        }
    }

    public class DisplaySelector : TabViewModel
    {
        private ObservableCollection<DisplayViewModel> _displayViewModels =
            new ObservableCollection<DisplayViewModel>();
        public ObservableCollection<DisplayViewModel> DisplayViewModels
        {
            get { return _displayViewModels; }
            set
            {
                if (_displayViewModels != value)
                {
                    _displayViewModels = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}