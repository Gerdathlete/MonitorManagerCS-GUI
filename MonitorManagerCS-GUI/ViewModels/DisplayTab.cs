using CommunityToolkit.Mvvm.Input;
using MonitorManagerCS_GUI.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MonitorManagerCS_GUI.ViewModels
{
    public class DisplayTab : TabViewModel
    {
        private ObservableCollection<VCPCodeChart> _vcpCodeCharts;
        public ObservableCollection<VCPCodeChart> VCPCodeCharts
        {
            get => _vcpCodeCharts;
            set
            {
                if (_vcpCodeCharts != value)
                {
                    _vcpCodeCharts = value;
                    OnPropertyChanged(nameof(VCPCodeCharts));
                }
            }
        }
        private VCPCodeChart _selectedVCPCodeChart;
        public VCPCodeChart SelectedVCPCodeChart
        {
            get => _selectedVCPCodeChart;
            set
            {
                if (value != _selectedVCPCodeChart)
                {
                    _selectedVCPCodeChart = value;
                    OnPropertyChanged(nameof(SelectedVCPCodeChart));
                }
            }
        }
        private DisplayManager _displayManager;
        public DisplayManager DisplayManager
        {
            get => _displayManager;
            set
            {
                if (_displayManager != value)
                {
                    _displayManager = value;
                    TabName = GetTabName(_displayManager.Display);
                    MakeVCPCodeCharts(_displayManager.VCPCodeControllers);
                }
            }
        }

        public DisplayTab(DisplayManager displayManager)
        {
            DisplayManager = displayManager;
        }

        private void MakeVCPCodeCharts(List<VCPCodeController> vcpControllers)
        {
            var vcpCharts = new List<VCPCodeChart>();
            foreach (var vcpController in vcpControllers)
            {
                vcpCharts.Add(new VCPCodeChart(vcpController));
            }

            VCPCodeCharts = new ObservableCollection<VCPCodeChart>(vcpCharts);
        }

        public static string GetTabName(DisplayInfo display)
        {
            return $"{display.ShortID} (SN: {display.SerialNumber})";
        }

        public void SelectVCPCode(string code)
        {
            if (_vcpCodeCharts == null || !_vcpCodeCharts.Any()) { return; }

            bool hasCode = false;

            foreach (var vcpCodeChart in _vcpCodeCharts)
            {
                if (vcpCodeChart.VCPController.Code == code)
                {
                    SelectedVCPCodeChart = vcpCodeChart;
                    hasCode = true;
                    return;
                }
            }

            if (!hasCode)
            {
                SelectedVCPCodeChart = _vcpCodeCharts.First();
            }
        }

        private RelayCommand saveAndApplyCommand;
        public ICommand SaveAndApplyCommand
        {
            get
            {
                if (saveAndApplyCommand == null)
                {
                    saveAndApplyCommand = new RelayCommand(SaveAndApply);
                }

                return saveAndApplyCommand;
            }
        }

        private void SaveAndApply()
        {
            //Store all the values that were changed
            foreach (var vcpCodeChart in VCPCodeCharts)
            {
                vcpCodeChart.UpdateVCPController();
            }

            //Save to config file
            _displayManager.Save();
        }
    }
}
