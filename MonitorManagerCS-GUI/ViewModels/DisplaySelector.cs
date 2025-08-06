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

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (!Equals(_isLoading, value))
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                    OnPropertyChanged(nameof(IsNotLoading));
                }
            }
        }
        public bool IsNotLoading => !IsLoading;
    }
}
