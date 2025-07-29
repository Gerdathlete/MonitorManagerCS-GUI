using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace MonitorManagerCS_GUI.ViewModels
{
    public class DisplaysTab : TabViewModel
    {
        public ObservableCollection<DisplayViewModel> Displays;
        public DisplaysTab()
        {
            Displays = new ObservableCollection<DisplayViewModel>()
            {
                new DisplayViewModel()
                {
                    Label = "Display 1",
                    X = 100,
                    Y = 100,
                },
                new DisplayViewModel()
                {
                    Label = "Display 2",
                    X = 300,
                    Y = 100,
                },
            };
        }
    }
}