using MonitorManagerCS_GUI.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
        //public TimeChartDraggable Chart { get; }
        private DisplayInfo _display;
        public DisplayInfo Display
        {
            get => _display;
            set
            {
                if (_display != value)
                {
                    _display = value;
                    TabName = GetTabName(_display);
                }
            }
        }

        public DisplayTab(DisplayInfo display)
            : this()
        {
            Display = display;
            MakeVCPCodeCharts(display.VCPCodes);
        }
        public DisplayTab() { }

        private void MakeVCPCodeCharts(List<VCPCode> vcpCodes)
        {
            var writableCodes = vcpCodes.Where(vcpCode => vcpCode.IsWritable).ToList();

            var codeCharts = new List<VCPCodeChart>();
            foreach (var vcpCode in writableCodes)
            {
                var codeControl = new VCPCodeControl(vcpCode);
                codeCharts.Add(new VCPCodeChart(codeControl));
            }

            VCPCodeCharts = new ObservableCollection<VCPCodeChart>(codeCharts);
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
                if (vcpCodeChart.VCPCode.Code == code)
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
    }
}
