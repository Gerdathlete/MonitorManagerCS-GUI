using System.ComponentModel;

namespace MonitorManagerCS_GUI.ViewModels
{
    public class TabViewModel : INotifyPropertyChanged
    {
        private string _tabName;
        public string TabName
        {
            get => _tabName;
            set
            {
                if (_tabName != value)
                {
                    _tabName = value;
                    OnPropertyChanged(nameof(TabName));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
