using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MonitorManagerCS_GUI.Controls
{
    /// <summary>
    /// Interaction logic for ExplorableCanvas.xaml
    /// </summary>
    public partial class ExplorableCanvas : Panel
    {
        private Thickness _contentsPadding = new Thickness(100);
        public Thickness ContentsPadding
        {
            get => _contentsPadding;
            set
            {
                if (_contentsPadding != value)
                {
                    _contentsPadding = value;
                    InvalidateArrange();
                }
            }
        }

        public static readonly DependencyProperty XProperty;
        public static readonly DependencyProperty YProperty;

        private Point _lastMousePos;
        private bool _isPanning = false;
        private double _minScaleFactor = 1.0;
        private const double _maxScaleFactor = 100.0;
        private Rect _providedBounds;
        private Rect _untransformedBounds;
        private Rect _bounds;
        private double _currentScale;
        private double _xOffset = 0.0;
        private double _yOffset = 0.0;

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

            Loaded += OnLoaded;
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

                var globalMousePos = e.GetPosition(null);
                _lastMousePos = globalMousePos;
                Mouse.Capture(this);
            }

            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isPanning)
            {
                Point globalMousePos = e.GetPosition(null);
                Vector delta = globalMousePos - _lastMousePos;
                _lastMousePos = globalMousePos;

                PanTransform.X += delta.X;
                PanTransform.Y += delta.Y;

                MoveViewInBounds();
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
            Point firstScreenSpacePos = RenderTransform.Transform(pos);

            SetScale(scalingFactor);

            Point secondScreenSpacePos = RenderTransform.Transform(pos);

            //this is how much the camera panned when zooming
            Vector screenSpaceDelta = secondScreenSpacePos - firstScreenSpacePos;

            //undo the pan
            PanTransform.X -= screenSpaceDelta.X;
            PanTransform.Y -= screenSpaceDelta.Y;

            MoveViewInBounds();
        }
        private void SetScale(double scalingFactor)
        {
            if (scalingFactor < _minScaleFactor)
            {
                scalingFactor = _minScaleFactor;
            }

            if (scalingFactor > _maxScaleFactor)
            {
                scalingFactor = _maxScaleFactor;
            }

            ZoomScale.ScaleX = scalingFactor;
            ZoomScale.ScaleY = scalingFactor;

            _currentScale = scalingFactor;
        }

        public void CenterAboutPoint(Point pos)
        {
            var transformedPos = RenderTransform.Transform(pos);

            PanTransform.X -= transformedPos.X;
            PanTransform.Y -= transformedPos.Y;

            PanTransform.X += _providedBounds.Width / 2;
            PanTransform.Y += _providedBounds.Height / 2;

            MoveViewInBounds();
        }

        private void MoveViewInBounds()
        {
            _bounds = GetBounds();

            if (_bounds.Left > _providedBounds.Left)
            {
                double diff = _providedBounds.Left - _bounds.Left;
                PanTransform.X += diff;
            }

            if (_bounds.Top > _providedBounds.Top)
            {
                double diff = _providedBounds.Top - _bounds.Top;
                PanTransform.Y += diff;
            }

            if (_bounds.Right < _providedBounds.Right)
            {
                double diff = _providedBounds.Right - _bounds.Right;
                PanTransform.X += diff;
            }

            if (_bounds.Bottom < _providedBounds.Bottom)
            {
                double diff = _providedBounds.Bottom - _bounds.Bottom;
                PanTransform.Y += diff;
            }

            _bounds = GetBounds();
        }

        private Rect GetBounds()
        {
            return RenderTransform.TransformBounds(_untransformedBounds);
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

        public double GetPaddedX(UIElement element)
        {
            double x = 0.0;

            double newX = GetX(element);
            if (!double.IsNaN(newX))
            {
                x = newX + ContentsPadding.Left;
            }

            return x;
        }
        public double GetPaddedY(UIElement element)
        {
            double y = 0.0;

            double newY = GetY(element);
            if (!double.IsNaN(newY))
            {
                y = newY + ContentsPadding.Top;
            }

            return y;
        }

        private Rect GetChildrenBounds()
        {
            //default bounds if no children
            Rect bounds = new Rect(0, 0,
                ContentsPadding.Left + ContentsPadding.Right,
                ContentsPadding.Top + ContentsPadding.Bottom);

            var minX = bounds.Left;
            var minY = bounds.Top;
            var maxX = bounds.Right;
            var maxY = bounds.Bottom;

            foreach (UIElement internalChild in InternalChildren)
            {
                if (internalChild == null)
                {
                    continue;
                }

                double childX = GetPaddedX(internalChild);
                double childY = GetPaddedY(internalChild);

                double childMinX = GetX(internalChild);
                double childMinY = GetY(internalChild);

                minX = Math.Min(minX, childMinX);
                minY = Math.Min(minY, childMinY);

                double childMaxX = childX + internalChild.DesiredSize.Width
                    + ContentsPadding.Right;
                double childMaxY = childY + internalChild.DesiredSize.Height
                    + ContentsPadding.Bottom;

                maxX = Math.Max(maxX, childMaxX);
                maxY = Math.Max(maxY, childMaxY);
            }

            if (minX < 0)
            {
                _xOffset = -minX;
            }

            if (minY < 0)
            {
                _yOffset = -minY;
            }

            bounds = new Rect(_xOffset + minX, _yOffset + minY, maxX - minX, maxY - minY);

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                throw new Exception($"{nameof(ExplorableCanvas)}.{nameof(GetChildrenBounds)} " +
                    $"returned an invalid width or height!");
            }

            return bounds;
        }

        /// <summary>
        /// Positions the child elements of this ExplorableCanvas
        /// </summary>
        /// <param name="availableSpace">The size allocated for positioning child elements</param>
        /// <returns>The total size used after positioning child elements</returns>
        protected override Size ArrangeOverride(Size availableSpace)
        {
            _providedBounds = new Rect(availableSpace);

            _untransformedBounds = GetChildrenBounds();
            var bounds = _untransformedBounds;

            foreach (UIElement internalChild in InternalChildren)
            {
                double childX = GetPaddedX(internalChild) + _xOffset;
                double childY = GetPaddedY(internalChild) + _yOffset;

                internalChild.Arrange(new Rect(new Point(childX, childY),
                    internalChild.DesiredSize));
            }

            _minScaleFactor = Math.Max(
                _providedBounds.Width / bounds.Width,
                _providedBounds.Height / bounds.Height);

            SetScale(_minScaleFactor);
            CenterAboutPoint(new Point(bounds.Right / 2, bounds.Left / 2));

            //Always take up the full available space, even if the contents are smaller, because
            //the canvas is scaled to fit in the window
            var arrangeSize = new Size(Math.Max(availableSpace.Width, bounds.Width),
                Math.Max(availableSpace.Height, bounds.Height));

            return arrangeSize;
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

        private static bool IsDoubleFiniteOrNaN(object value)
        {
            double d = (double)value;
            return !double.IsInfinity(d);
        }

        #region Debugging Tools

#if DEBUG
        private readonly bool _debug = true;
#else
        private readonly bool _debug = false;
#endif

        private DebugAdorner _debugAdorner;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_debug) return;

            var adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer != null)
            {
                _debugAdorner = new DebugAdorner(this);
                adornerLayer.Add(_debugAdorner);
            }
        }

        public void DebugDrawPointOnCanvas(Point pt, Brush brush, double radius = 3)
        {
            if (!_debug) return;

            pt = RenderTransform.Transform(pt);

            DebugDrawPoint(pt, brush, radius);
        }
        public void DebugDrawPoint(Point pt, Brush brush, double radius = 3)
        {
            if (!_debug) return;

            _debugAdorner?.DrawActions.Add(dc =>
            {
                dc.DrawEllipse(brush, new Pen(Brushes.Black, 1), pt, radius, radius);
            });
            _debugAdorner?.Invalidate();
        }

        public void DebugDrawLineOnCanvas(Point from, Point to, Brush brush, double thickness = 1)
        {
            if (!_debug) return;

            from = RenderTransform.Transform(from);
            to = RenderTransform.Transform(to);

            DebugDrawLine(from, to, brush, thickness);
        }
        public void DebugDrawLine(Point from, Point to, Brush brush, double thickness = 1)
        {
            if (!_debug) return;

            _debugAdorner?.DrawActions.Add(dc =>
            {
                var pen = new Pen(brush, thickness) { DashStyle = DashStyles.Dash };
                dc.DrawLine(pen, from, to);
            });
            _debugAdorner?.Invalidate();
        }

        public void DebugDrawRectOnCanvas(Rect rect, Brush fill, Brush outline, double thickness = 1)
        {
            if (!_debug) return;

            rect = RenderTransform.TransformBounds(rect);

            DebugDrawRect(rect, fill, outline, thickness);
        }
        public void DebugDrawRect(Rect rect, Brush fill, Brush outline, double thickness = 1)
        {
            if (!_debug) return;

            _debugAdorner?.DrawActions.Add(dc =>
            {
                var pen = new Pen(outline, thickness) { DashStyle = DashStyles.Dash };
                dc.DrawRectangle(fill, pen, rect);
            });
            _debugAdorner?.Invalidate();
        }

        public void ClearDebugDrawings()
        {
            if (!_debug) return;

            _debugAdorner?.DrawActions.Clear();
            _debugAdorner?.Invalidate();
        }

        #endregion
    }
}
