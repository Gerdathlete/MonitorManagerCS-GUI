using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MonitorManagerCS_GUI.Controls
{
    /// <summary>
    /// Interaction logic for ExplorableCanvas.xaml
    /// </summary>
    public partial class ExplorableCanvas : Canvas
    {
        public Thickness ContentsPadding { get; set; } = new Thickness(100);

        private Point lastMousePos;
        private bool isPanning = false;

        public ExplorableCanvas()
        {
            InitializeComponent();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? 1.1 : 1 / 1.1;

            var mousePos = e.GetPosition(this);

            ZoomScale.CenterX = mousePos.X;
            ZoomScale.CenterY = mousePos.Y;

            ZoomScale.ScaleX *= zoomFactor;
            ZoomScale.ScaleY *= zoomFactor;

            e.Handled = true;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isPanning = true;

                // Store mouse pos in screen coordinates
                lastMousePos = e.GetPosition(null);
                Mouse.Capture(this);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isPanning)
            {
                Point currentMousePos = e.GetPosition(null);
                Vector delta = currentMousePos - lastMousePos;
                lastMousePos = currentMousePos;

                PanTransform.X += delta.X;
                PanTransform.Y += delta.Y;

                if (PanTransform.X > 0) PanTransform.X = 0;
                if (PanTransform.Y > 0) PanTransform.Y = 0;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isPanning = false;
                Mouse.Capture(null);
            }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            foreach (UIElement internalChild in InternalChildren)
            {
                if (internalChild == null)
                {
                    continue;
                }

                double x = 0.0;
                double y = 0.0;
                double left = GetLeft(internalChild) + ContentsPadding.Left;
                if (!double.IsNaN(left))
                {
                    x = left;
                }
                else
                {
                    double right = GetRight(internalChild) + ContentsPadding.Right;
                    if (!double.IsNaN(right))
                    {
                        x = arrangeSize.Width - internalChild.DesiredSize.Width - right;
                    }
                }

                double top = GetTop(internalChild) + ContentsPadding.Top;
                if (!double.IsNaN(top))
                {
                    y = top;
                }
                else
                {
                    double bottom = GetBottom(internalChild) + ContentsPadding.Bottom;
                    if (!double.IsNaN(bottom))
                    {
                        y = arrangeSize.Height - internalChild.DesiredSize.Height - bottom;
                    }
                }

                internalChild.Arrange(new Rect(new Point(x, y), internalChild.DesiredSize));
            }

            return arrangeSize;
        }
    }
}
