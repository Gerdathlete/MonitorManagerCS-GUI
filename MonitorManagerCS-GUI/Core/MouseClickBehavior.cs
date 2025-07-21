using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace MonitorManagerCS_GUI
{
    public class MouseClickBehavior : Behavior<UIElement>
    {
        public ICommand LeftClickCommand
        {
            get => (ICommand)GetValue(LeftClickCommandProperty);
            set => SetValue(LeftClickCommandProperty, value);
        }

        public static readonly DependencyProperty LeftClickCommandProperty =
            DependencyProperty.Register(nameof(LeftClickCommand), typeof(ICommand), typeof(MouseClickBehavior));

        public ICommand RightClickCommand
        {
            get => (ICommand)GetValue(RightClickCommandProperty);
            set => SetValue(RightClickCommandProperty, value);
        }

        public static readonly DependencyProperty RightClickCommandProperty =
            DependencyProperty.Register(nameof(RightClickCommand), typeof(ICommand), typeof(MouseClickBehavior));

        protected override void OnAttached()
        {
            AssociatedObject.MouseDown += OnMouseDown;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= OnMouseDown;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && LeftClickCommand?.CanExecute(e) == true)
                LeftClickCommand.Execute(e);

            if (e.ChangedButton == MouseButton.Right && RightClickCommand?.CanExecute(e) == true)
                RightClickCommand.Execute(e);
        }
    }
}

