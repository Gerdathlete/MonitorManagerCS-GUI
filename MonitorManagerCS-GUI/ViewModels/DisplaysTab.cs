using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;

namespace MonitorManagerCS_GUI.ViewModels
{
    public class DisplaysTab : TabViewModel
    {
        private ObservableCollection<DisplayViewModel> _displays;
        public ObservableCollection<DisplayViewModel> Displays
        {
            get { return _displays; }
            set
            {
                if (_displays != value)
                {
                    _displays = value;
                    OnPropertyChanged(nameof(Displays));
                }
            }
        }
        public DisplaysTab()
        {
            Displays = new ObservableCollection<DisplayViewModel>()
            {
                new DisplayViewModel()
                {
                    Label = "Display 1",
                    Bounds = new Rect(0.0,0.0,1920.0,1080.0)
                },
                new DisplayViewModel()
                {
                    Label = "Display 2",
                    Bounds = new Rect(1920.0+20.0, 0.0, 1920.0, 1080.0)
                },
                new DisplayViewModel()
                {
                    Label = "CenterDisplay",
                    Bounds = new Rect(1920.0+10.0-50.0, 1080.0/2.0-50.0, 100.0, 100.0)
                },
            };
        }
    }
}