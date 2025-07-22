using System.Collections.ObjectModel;

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
    }
}
