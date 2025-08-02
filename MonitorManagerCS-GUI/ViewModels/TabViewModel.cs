using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MonitorManagerCS_GUI.ViewModels
{
    public abstract class TabViewModel : INotifyPropertyChanged
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
