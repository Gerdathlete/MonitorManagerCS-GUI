using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MonitorManagerCS_GUI.Core
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bValue = value is bool v && v;
            return bValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return (value is Visibility visibility && visibility != Visibility.Visible);
        }
    }
}
