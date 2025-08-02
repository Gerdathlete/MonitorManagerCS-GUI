using MonitorManagerCS_GUI.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace MonitorManagerCS_GUI.ViewModels
{
    public class DisplaysTab : TabViewModel
    {
        private List<DisplayInfo> _displays;
        public List<DisplayInfo> Displays
        {
            get { return _displays; }
            set
            {
                if (_displays != value)
                {
                    _displays = value;
                    DisplayViewModels = GetVMsFromDisplays(value);
                }
            }
        }

        private ObservableCollection<DisplayViewModel> _displayViewModels;
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

        public double Scale { get; set; } = 0.1;

        public DisplaysTab()
        {
            DisplayViewModels = new ObservableCollection<DisplayViewModel>();
            TabName = "Displays";
        }

        private ObservableCollection<DisplayViewModel> GetVMsFromDisplays(List<DisplayInfo> displays)
        {
            return new ObservableCollection<DisplayViewModel>(
                displays.Select(d => new DisplayViewModel(d, Scale)));
        }
    }
}