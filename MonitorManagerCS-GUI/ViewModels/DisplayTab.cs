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
        public DisplayTab()
        {
            VCPCodes = new ObservableCollection<VCPCode>();
            Chart = new TimeChartDraggable();
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
