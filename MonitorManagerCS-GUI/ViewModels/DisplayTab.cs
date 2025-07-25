using MonitorManagerCS_GUI.Core;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonitorManagerCS_GUI.ViewModels
{
    public class DisplayTab : TabViewModel
    {
        private ObservableCollection<VCPCode> _VCPCodes;
        public ObservableCollection<VCPCode> VCPCodes
        {
            get => _VCPCodes;
            set
            {
                if (_VCPCodes != value)
                {
                    _VCPCodes = value;
                    OnPropertyChanged(nameof(VCPCodes));
                }
            }
        }
        private VCPCode _selectedVCPCode;
        public VCPCode SelectedVCPCode
        {
            get => _selectedVCPCode;
            set
            {
                if (value != _selectedVCPCode)
                {
                    _selectedVCPCode = value;
                    OnPropertyChanged(nameof(SelectedVCPCode));
                }
            }
        }
        public TimeChartDraggable Chart { get; set; }
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
                    VCPCodes = new ObservableCollection<VCPCode>(_display.VCPCodes);
                }
            } 
        }

        public DisplayTab(DisplayInfo display)
            :this()
        {
            Display = display;
        }
        public DisplayTab()
        {
            VCPCodes = new ObservableCollection<VCPCode>();
            Chart = new TimeChartDraggable();
        }

        public static string GetTabName(DisplayInfo display)
        {
            return $"{display.ShortID} (SN: {display.SerialNumber})";
        }

        public void SelectVCPCode(string code)
        {
            if (_VCPCodes == null || !_VCPCodes.Any()) { return; }

            bool hasCode = false;

            foreach(var vcpCode in _VCPCodes)
            {
                if (vcpCode.Code == code)
                {
                    SelectedVCPCode = vcpCode;
                    hasCode = true;
                    return;
                }
            }

            if (!hasCode)
            {
                SelectedVCPCode = _VCPCodes.First();
            }
        }
    }
}
