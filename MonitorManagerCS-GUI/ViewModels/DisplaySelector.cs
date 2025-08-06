using System.Collections.ObjectModel;

namespace MonitorManagerCS_GUI.ViewModels
{
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
