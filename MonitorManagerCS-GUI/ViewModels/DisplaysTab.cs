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
            double scale = 0.1;

            Displays = new ObservableCollection<DisplayViewModel>()
            {
                new DisplayViewModel()
                {
                    Label = "Display 1",
                    Scale = scale,
                    Bounds = new Rect(0.0,0.0,1920.0,1080.0),
                },
                new DisplayViewModel()
                {
                    Label = "Display 2",
                    Scale = scale,
                    Bounds = new Rect(1920.0+20.0, 0.0, 1920.0, 1080.0),
                },
            };
        }
    }
}