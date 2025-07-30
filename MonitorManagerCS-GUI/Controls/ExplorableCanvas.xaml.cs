using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MonitorManagerCS_GUI.Controls
{
    /// <summary>
    /// Interaction logic for ExplorableCanvas.xaml
    /// </summary>
    public partial class ExplorableCanvas : Panel
    {
        public Thickness ContentsPadding { get; set; } = new Thickness(100);

        public static readonly DependencyProperty XProperty;
        public static readonly DependencyProperty YProperty;

        private Point _lastMousePos;
        private bool _isPanning = false;
        private double _minScaleFactor = 1.0;
        private Rect _providedBounds;
        private Size _size;
        private Rect _transformedBounds;
        private double _currentScale;

        static ExplorableCanvas()
        {
            XProperty = DependencyProperty.RegisterAttached(
                "X",
                typeof(double),
                typeof(ExplorableCanvas),
                new FrameworkPropertyMetadata(
                    double.NaN,
                    FrameworkPropertyMetadataOptions.AffectsArrange),
                IsDoubleFiniteOrNaN);

            YProperty = DependencyProperty.RegisterAttached(
                "Y",
                typeof(double),
                typeof(ExplorableCanvas),
                new FrameworkPropertyMetadata(
                    double.NaN,
                    FrameworkPropertyMetadataOptions.AffectsArrange),
                IsDoubleFiniteOrNaN);
        }

        public ExplorableCanvas()
        {
            InitializeComponent();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? 1.1 : 1 / 1.1;

            var mousePos = e.GetPosition(this);

            ZoomAboutPoint(mousePos, zoomFactor);

            e.Handled = true;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isPanning = true;

                // Store mouse pos in screen coordinates
                _lastMousePos = e.GetPosition(null);
                Mouse.Capture(this);
            }

            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isPanning)
            {
                Point currentMousePos = e.GetPosition(null);
                Vector delta = currentMousePos - _lastMousePos;
                _lastMousePos = currentMousePos;

                PanTransform.X += delta.X;
                PanTransform.Y += delta.Y;

                MoveInBounds();
            }

            e.Handled = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isPanning = false;
                Mouse.Capture(null);
            }

            e.Handled = true;
        }

        public void ZoomAboutPoint(Point pos, double zoomFactor)
        {
            var scalingFactor = _currentScale * zoomFactor;
            ScaleAboutPoint(pos, scalingFactor);
        }

        public void ScaleAboutPoint(Point pos, double scalingFactor)
        {
            if (scalingFactor < _minScaleFactor)
            {
                scalingFactor = _minScaleFactor;
            }

            ZoomScale.ScaleX = scalingFactor;
            ZoomScale.ScaleY = scalingFactor;

            _currentScale = scalingFactor;

            CenterAboutPoint(pos);

            MoveInBounds();
        }

        public void CenterAboutPoint(Point pos)
        {
            var transformedPos = RenderTransform.Transform(pos);

            PanTransform.X -= transformedPos.X;
            PanTransform.Y -= transformedPos.Y;

            PanTransform.X += _providedBounds.Width / 2;
            PanTransform.Y += _providedBounds.Height / 2;

            MoveInBounds();
        }

        private void MoveInBounds()
        {
            _transformedBounds = GetTransformedBounds();

            if (_transformedBounds.Left > _providedBounds.Left)
            {
                double diff = _providedBounds.Left - _transformedBounds.Left;
                PanTransform.X += diff;
            }

            if (_transformedBounds.Top > _providedBounds.Top)
            {
                double diff = _providedBounds.Top - _transformedBounds.Top;
                PanTransform.Y += diff;
            }

            if (_transformedBounds.Right < _providedBounds.Right)
            {
                double diff = _providedBounds.Right - _transformedBounds.Right;
                PanTransform.X += diff;
            }

            if (_transformedBounds.Bottom < _providedBounds.Bottom)
            {
                double diff = _providedBounds.Bottom - _transformedBounds.Bottom;
                PanTransform.Y += diff;
            }
        }

        private Rect GetTransformedBounds()
        {
            return RenderTransform.TransformBounds(new Rect(_size));
        }

        public static void SetX(UIElement element, double value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(XProperty, value);
        }
        public static double GetX(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (double)element.GetValue(XProperty);
        }
        public static void SetY(UIElement element, double value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(YProperty, value);
        }
        public static double GetY(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (double)element.GetValue(YProperty);
        }

        /// <summary>
        /// Positions the child elements of this ExplorableCanvas
        /// </summary>
        /// <param name="arrangeSize">The size allocated for positioning child elements</param>
        /// <returns>The total size used after positioning child elements</returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            _providedBounds = new Rect(arrangeSize);

            double maxX = _providedBounds.Width; //default if no children
            double maxY = _providedBounds.Height;

            foreach (UIElement internalChild in InternalChildren)
            {
                if (internalChild == null)
                {
                    continue;
                }

                double childX = 0.0;
                double childY = 0.0;

                double newX = GetX(internalChild) + ContentsPadding.Left;
                if (!double.IsNaN(newX))
                {
                    childX = newX;
                }

                double newY = GetY(internalChild) + ContentsPadding.Top;
                if (!double.IsNaN(newY))
                {
                    childY = newY;
                }

                internalChild.Arrange(new Rect(new Point(childX, childY),
                    internalChild.DesiredSize));

                double childMaxX = childX + internalChild.DesiredSize.Width
                    + ContentsPadding.Right;
                double childMaxY = childY + internalChild.DesiredSize.Height
                    + ContentsPadding.Bottom;

                maxX = Math.Max(maxX, childMaxX);
                maxY = Math.Max(maxY, childMaxY);
            }

            _size = new Size(maxX, maxY);

            _minScaleFactor = Math.Max(
                _providedBounds.Width/_size.Width, 
                _providedBounds.Height/_size.Height);

            ScaleAboutPoint(new Point(0,0), _minScaleFactor);

            return _size;
        }

        /// <summary>
        /// Measures the child elements of an ExplorableCanvas in anticipation of arranging them
        /// during ExplorableCanvas.ArrangeOverride
        /// </summary>
        /// <param name="constraint">An upper limit System.Windows.Size that should not be
        /// exceeded.</param>
        /// <returns>The size requirements of the ExplorableCanvas based on its children's sizes
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            foreach (UIElement internalChild in InternalChildren)
            {
                internalChild?.Measure(availableSize);
            }

            return default;
        }

        /// <summary>
        /// Returns a clipping geometry that indicates the area that will be clipped if the
        /// System.Windows.UIElement.ClipToBounds property is set to true.
        /// </summary>
        /// <param name="layoutSlotSize">The available size of the element.</param>
        /// <returns>A System.Windows.Media.Geometry that represents the area that is clipped if 
        /// System.Windows.UIElement.is true.</returns>
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            if (ClipToBounds)
            {
                return new RectangleGeometry(new Rect(RenderSize));
            }

            return null;
        }

        private static void OnPositioningChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {

        }

        private static bool IsDoubleFiniteOrNaN(object value)
        {
            double d = (double)value;
            return !double.IsInfinity(d);
        }
    }
}
