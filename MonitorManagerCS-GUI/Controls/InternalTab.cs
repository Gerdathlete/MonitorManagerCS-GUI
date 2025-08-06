using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MonitorManagerCS_GUI.Controls
{
    public class InternalTab : ContentControl
    {
        static InternalTab()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InternalTab), new FrameworkPropertyMetadata(typeof(InternalTab)));
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(string),
                typeof(InternalTab),
                new PropertyMetadata(string.Empty));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty HeaderFontSizeProperty =
            DependencyProperty.Register(
                nameof(HeaderFontSize),
                typeof(double),
                typeof(InternalTab),
                new PropertyMetadata(SystemFonts.MessageFontSize));

        public double HeaderFontSize
        {
            get => (double)GetValue(HeaderFontSizeProperty);
            set => SetValue(HeaderFontSizeProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(InternalTab),
                new PropertyMetadata(null));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly RoutedEvent ExitButtonPressedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(ExitButtonPressed),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(InternalTab));

        public event RoutedEventHandler ExitButtonPressed
        {
            add { AddHandler(ExitButtonPressedEvent, value); }
            remove { RemoveHandler(ExitButtonPressedEvent, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_ExitButton") is Button exitButton)
            {
                exitButton.Click -= ExitButton_Click;
                exitButton.Click += ExitButton_Click;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ExitButtonPressedEvent));
        }
    }
}
