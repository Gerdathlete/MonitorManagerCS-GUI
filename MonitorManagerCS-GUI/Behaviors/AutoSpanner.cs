using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MonitorManagerCS_GUI.Behaviors
{
    public static class AutoSpanner
    {
        public static readonly DependencyProperty SpanAllRowsProperty =
            DependencyProperty.RegisterAttached(
                "SpanAllRows",
                typeof(bool),
                typeof(AutoSpanner),
                new PropertyMetadata(false, OnSpanAllRowsChanged));

        public static void SetSpanAllRows(UIElement element, bool value)
        {
            element.SetValue(SpanAllRowsProperty, value);
        }

        public static bool GetSpanAllRows(UIElement element)
        {
            return (bool)element.GetValue(SpanAllRowsProperty);
        }

        private static void OnSpanAllRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe && e.NewValue is bool shouldSpan && shouldSpan)
            {
                fe.Loaded -= SpanAllRows;
                fe.Loaded += SpanAllRows;
            }
            else if (d is FrameworkElement fe2 && e.NewValue is bool newVal && !newVal)
            {
                fe2.Loaded -= SpanAllRows;
            }
        }

        private static void SpanAllRows(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe &&
                VisualTreeHelper.GetParent(fe) is Grid grid)
            {
                int rowCount = grid.RowDefinitions.Count;
                if (rowCount > 0)
                {
                    Grid.SetRowSpan(fe, rowCount);
                }

                fe.Loaded -= SpanAllRows;
            }
        }

        public static readonly DependencyProperty SpanAllColumnsProperty =
            DependencyProperty.RegisterAttached(
                "SpanAllColumns",
                typeof(bool),
                typeof(AutoSpanner),
                new PropertyMetadata(false, OnSpanAllColumnsChanged));

        public static void SetSpanAllColumns(UIElement element, bool value)
        {
            element.SetValue(SpanAllColumnsProperty, value);
        }

        public static bool GetSpanAllColumns(UIElement element)
        {
            return (bool)element.GetValue(SpanAllColumnsProperty);
        }

        private static void OnSpanAllColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe && e.NewValue is bool shouldSpan && shouldSpan)
            {
                fe.Loaded -= SpanAllColumns;
                fe.Loaded += SpanAllColumns;
            }
            else if (d is FrameworkElement fe2 && e.NewValue is bool newVal && !newVal)
            {
                fe2.Loaded -= SpanAllColumns;
            }
        }

        private static void SpanAllColumns(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe &&
                VisualTreeHelper.GetParent(fe) is Grid grid)
            {
                int columnCount = grid.ColumnDefinitions.Count;
                if (columnCount > 0)
                {
                    Grid.SetColumnSpan(fe, columnCount);
                }

                fe.Loaded -= SpanAllColumns;
            }
        }
    }

}
