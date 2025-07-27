using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MonitorManagerCS_GUI.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace MonitorManagerCS_GUI.ViewModels
{
    public partial class TimeChartDraggable : INotifyPropertyChanged
    {
        private ObservableCollection<ObservablePoint> _points;
        public ObservableCollection<ObservablePoint> Points
        {
            get => _points;
            set
            {
                if (_points != value)
                {
                    var sortedPoints = new ObservableCollection<ObservablePoint>
                        (value.OrderBy(p => p.X));
                    if (_points != sortedPoints)
                    {
                        _points = sortedPoints;
                        _series1.Values = _points;
                        OnPropertyChanged(nameof(Points));
                    }
                }
            }
        }
        private readonly LineSeries<ObservablePoint> _series1;
        public ISeries[] DraggableSeries { get; }
        public Axis TimeAxis { get; }
        public Axis YAxis { get; set; }
        public Axis[] XAxes { get; }
        public Axis[] YAxes { get; }
        public double XSnap { get; set; } = .25;
        public double YSnap { get; set; } = 1;
        private TooltipPosition _toolTipPos = TooltipPosition.Top;
        public TooltipPosition TooltipPos
        {
            get => _toolTipPos;
            set
            {
                if (_toolTipPos != value)
                {
                    _toolTipPos = value;
                    OnPropertyChanged(nameof(TooltipPos));
                }
            }
        }
        public IRelayCommand<PointerCommandArgs> PointerReleasedCommand { get; }
        public IRelayCommand<PointerCommandArgs> PointerMovedCommand { get; }
        public IRelayCommand<PointerCommandArgs> PointerPressedCommand { get; }

        private ObservablePoint _draggedPoint = null;
        private readonly TooltipPosition _prevTooltipPos;
        private const double _wrappingPointOffset = 10;

        public TimeChartDraggable()
        {
            PointerReleasedCommand = new RelayCommand<PointerCommandArgs>(OnMouseReleased);
            PointerMovedCommand = new RelayCommand<PointerCommandArgs>(OnMouseMoved);
            PointerPressedCommand = new RelayCommand<PointerCommandArgs>(OnMousePressed);

            _points = new ObservableCollection<ObservablePoint>();

            _series1 = new LineSeries<ObservablePoint>
            {
                Values = Points,
                GeometrySize = 10,
                LineSmoothness = 0,
            };

            DraggableSeries = new ISeries[] { _series1 };

            TimeAxis = new Axis
            {
                Name = "Time",
                MinLimit = 0,
                MaxLimit = 24,
                Labeler = v => DataFormatter.GetReadableTime(v),
                MinStep = 3,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 2 },
                SubseparatorsCount = 2,
                SubseparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 },
                ForceStepToMin = true
            };

            YAxis = new Axis
            {
                Name = "Value",
                MinLimit = 0,
                MaxLimit = 100,
            };

            XAxes = new[] { TimeAxis };

            YAxes = new[] { YAxis };

            _prevTooltipPos = TooltipPos;
        }

        /// <summary>
        /// Runs when a mouse button is pressed. Handles dragging points, adding new points,
        /// and deleting points.
        /// </summary>
        /// <param name="args"></param>
        public virtual void OnMousePressed(PointerCommandArgs args)
        {
            HideTooltips();

            var originalArgs = (MouseButtonEventArgs)args.OriginalEventArgs;
            var chart = (ICartesianChartView)args.Chart;
            LvcPointD mousePos = args.PointerPosition;

            //Find if we clicked a point and get that point.
            var clickedPoints = chart.GetPointsAt(mousePos);
            ObservablePoint clickedPoint = null;

            if (clickedPoints != null && clickedPoints.Any())
            {
                ChartPoint clickedChartPoint = clickedPoints.First();
                clickedPoint = (ObservablePoint)clickedChartPoint.Context.DataSource;
            }

            //Left Click
            if (originalArgs.ChangedButton == MouseButton.Left)
            {
                if (clickedPoint != null)
                {
                    //Drag existing point
                    _draggedPoint = clickedPoint;
                }
                else
                {
                    //Create new point and start dragging it
                    var mouseChartPos = chart.ScalePixelsToData(mousePos);

                    var newPoint = AddPoint(mouseChartPos);

                    UpdateWrappingPoints();

                    _draggedPoint = newPoint;
                }
            }

            //Right Click
            if (originalArgs.ChangedButton == MouseButton.Right)
            {
                if (clickedPoint != null)
                {
                    //Delete existing point
                    _points.Remove(clickedPoint);
                    UpdateWrappingPoints();

                    InvokePointsChanged();
                }
            }
        }

        /// <summary>
        /// Runs when the mouse is moved. Updates the position of points when they are dragged.
        /// </summary>
        /// <param name="args"></param>
        public virtual void OnMouseMoved(PointerCommandArgs args)
        {
            if (_draggedPoint == null) return;

            //Move the point to the mouse position with snapping
            var mousePos = args.PointerPosition;
            var chart = (ICartesianChartView)args.Chart;
            var mouseChartPos = chart.ScalePixelsToData(mousePos);

            var newPosNullable = GetValidPointLocation(mouseChartPos, _draggedPoint);
            if (newPosNullable == null) return;

            var newPos = (LvcPointD)newPosNullable;

            _draggedPoint.X = newPos.X;
            _draggedPoint.Y = newPos.Y;

            //Reorder points if needed
            UpdatePointIndex(_draggedPoint);

            UpdateWrappingPoints();
        }

        /// <summary>
        /// Runs when the left mouse button is released. Releases dragged points.
        /// </summary>
        /// <param name="args"></param>
        public virtual void OnMouseReleased(PointerCommandArgs args)
        {
            ShowTooltips();
            if (_draggedPoint == null) return;

            _draggedPoint = null;

            InvokePointsChanged();
        }

        /// <summary>
        /// Adds a point to the chart at the correct index so that the lines connect in order of 
        /// X value
        /// </summary>
        /// <param name="chartPos"></param>
        public ObservablePoint AddPoint(LvcPointD chartPos) => AddPoint(chartPos.X, chartPos.Y);
        /// <summary>
        /// Adds a point to the chart at the correct index so that the lines connect in order of 
        /// X value
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public ObservablePoint AddPoint(double x, double y)
        {
            LvcPointD? pointLocNullable = GetValidPointLocation(new LvcPointD(x, y));
            if (pointLocNullable == null) return null;

            var pointLoc = (LvcPointD)pointLocNullable;

            var newPoint = new ObservablePoint(pointLoc.X, pointLoc.Y);

            int pointIndex = GetPointIndex(newPoint.X);
            _points.Insert(pointIndex, newPoint);

            return newPoint;
        }

        public void UpdateWrappingPoints()
        {
            if (_points.Count < 1) return;

            if (TimeAxis.MinLimit == null || TimeAxis.MaxLimit == null)
            {
                Debug.WriteLine("Tried to update wrapping points, but the X axis didn't have its " +
                    "limits set.");
                return;
            }

            var minX = (double)TimeAxis.MinLimit;
            var maxX = (double)TimeAxis.MaxLimit;

            double leftPointX = minX - _wrappingPointOffset;
            double rightPointX = maxX + _wrappingPointOffset;

            ObservablePoint leftPoint;
            ObservablePoint rightPoint;

            bool hasWrappingPoints = _points.Any(p => p.X == leftPointX);
            bool onlyHasWrappingPoints = hasWrappingPoints && _points.Count < 3;

            if (onlyHasWrappingPoints)
            {
                _points.Clear();
                return;
            }

            if (!hasWrappingPoints)
            {
                leftPoint = new ObservablePoint(leftPointX, 0);
                rightPoint = new ObservablePoint(rightPointX, 0);


                _points.Insert(0, leftPoint);
                _points.Add(rightPoint);
            }
            else
            {
                leftPoint = _points[0];
                rightPoint = _points.Last();
            }

            SetWrappingPointsY(ref leftPoint, ref rightPoint);
        }

        public void SetWrappingPointsY(ref ObservablePoint leftPoint, ref ObservablePoint rightPoint)
        {
            double leftPointY = 0;
            double rightPointY = 0;

            //Get all the points that have X and Y values, excluding the wrapping points
            var points = _points.ToList();
            points.Remove(leftPoint);
            points.Remove(rightPoint);
            points.RemoveAll(p => p.X == null || p.Y == null);

            switch (points.Count)
            {
                //Don't do anything if there aren't any points
                case 0:
                    return;

                case 1:
                    leftPointY = (double)points[0].Y;
                    rightPointY = leftPointY;
                    break;

                default:
                    //Use interpolation to calculate y value of wrapping points
                    var point1 = points[0];
                    var point2 = points.Last();

                    var p1Y = (double)point1.Y;
                    var p1X = (double)point1.X;
                    var p2Y = (double)point2.Y;
                    var p2X = (double)point2.X;

                    var min = (double)TimeAxis.MinLimit;
                    var max = (double)TimeAxis.MaxLimit;

                    var yDiff = p1Y - p2Y;
                    var rightDist = max - p2X;
                    var leftDist = p1X - min;
                    var xDiff = leftDist + rightDist;

                    //If the points are on the edges of the chart, make the wrapping points have the
                    //same Y to hide the line
                    if (xDiff == 0)
                    {
                        leftPoint.Y = p1Y;
                        rightPoint.Y = p2Y;
                        return;
                    }

                    var leftScale = (leftDist + _wrappingPointOffset) / xDiff;
                    var rightScale = (rightDist + _wrappingPointOffset) / xDiff;

                    var leftOffset = yDiff * leftScale;
                    var rightOffset = yDiff * rightScale;

                    leftPointY = p1Y - leftOffset;
                    rightPointY = p2Y + rightOffset;
                    break;
            }

            leftPoint.Y = leftPointY;
            rightPoint.Y = rightPointY;
        }

        public virtual LvcPointD? GetValidPointLocation(LvcPointD chartPos,
            ObservablePoint point = null)
        {
            LvcPointD pointLocation = new LvcPointD(chartPos.X, chartPos.Y);

            //Snap to grid
            pointLocation.X = Math.Round(pointLocation.X / XSnap) * XSnap;
            pointLocation.Y = Math.Round(pointLocation.Y / YSnap) * YSnap;

            //Prevent point locations outside of the chart boundary
            if (TimeAxis.MaxLimit != null && pointLocation.X > TimeAxis.MaxLimit)
            {
                pointLocation.X = (double)TimeAxis.MaxLimit;
            }

            if (TimeAxis.MinLimit != null && pointLocation.X < TimeAxis.MinLimit)
            {
                pointLocation.X = (double)TimeAxis.MinLimit;
            }

            if (YAxis.MaxLimit != null && pointLocation.Y > YAxis.MaxLimit)
            {
                pointLocation.Y = (double)YAxis.MaxLimit;
            }

            if (YAxis.MinLimit != null && pointLocation.Y < YAxis.MinLimit)
            {
                pointLocation.Y = (double)YAxis.MinLimit;
            }

            //Prevent the location from being at the same X-value as another point

            int oldIndex = 0;
            if (point != null)
            {
                oldIndex = (point.MetaData != null) ? point.MetaData.EntityIndex : 0;
                _points.Remove(point);
            }

            int pointIndex = GetPointIndex(pointLocation.X);

            var replacedPointX = (pointIndex < _points.Count) ? _points[pointIndex].X : null;

            int leftDist = 0;
            int rightDist = 0;

            if (pointLocation.X == replacedPointX)
            {
                //Move left by the snap distance until we reach a location without a point
                leftDist++;
                double testLocation = pointLocation.X - XSnap;
                int testIndex = pointIndex - 1;
                var testPointX = (testIndex >= 0) ? _points[testIndex].X : null;

                while (testPointX == testLocation)
                {
                    leftDist++;
                    testLocation -= XSnap;
                    testIndex--;
                    testPointX = (testIndex >= 0) ? _points[testIndex].X : null;
                }

                //Move right by the snap distance until we reach a location without a point
                rightDist++;
                testLocation = pointLocation.X + XSnap;
                testIndex = pointIndex + 1;
                testPointX = (testIndex < _points.Count) ? _points[testIndex].X : null;

                while (testPointX == testLocation)
                {
                    rightDist++;
                    testLocation += XSnap;
                    testIndex++;
                    testPointX = (testIndex < _points.Count) ? _points[testIndex].X : null;
                }

                double newX;

                //Move the shorter distance to the new location
                if (leftDist >= rightDist)
                {
                    //Try moving right, check if out of bounds
                    newX = pointLocation.X + rightDist * XSnap;
                    if (newX > TimeAxis.MaxLimit)
                    {
                        //Try moving left, return null if still out of bounds
                        newX = pointLocation.X - leftDist * XSnap;
                        if (newX < TimeAxis.MinLimit)
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    //Try moving left, check if out of bounds
                    newX = pointLocation.X - leftDist * XSnap;
                    if (newX < TimeAxis.MinLimit)
                    {
                        //Try moving right, return null if still out of bounds
                        newX = pointLocation.X + rightDist * XSnap;
                        if (newX > TimeAxis.MaxLimit)
                        {
                            return null;
                        }
                    }
                }

                pointLocation.X = newX;
            }

            if (point != null)
            {
                _points.Insert(oldIndex, point);
            }

            return pointLocation;
        }

        public void UpdatePointIndex(ObservablePoint point)
        {
            _points.Remove(point);
            int newIndex = GetPointIndex(point.X);
            _points.Insert(newIndex, point);
        }

        public int GetPointIndex(double? pointX)
        {
            Debug.Assert(pointX != null);

            int pointIndex = 0;
            while (pointIndex < _points.Count && _points[pointIndex].X < pointX)
            { pointIndex++; }

            return pointIndex;
        }

        public void HideTooltips()
        {
            if (TooltipPos == TooltipPosition.Hidden) return;

            TooltipPos = TooltipPosition.Hidden;
        }

        public void ShowTooltips()
        {
            if (TooltipPos == _prevTooltipPos) return;

            TooltipPos = _prevTooltipPos;
        }

        /// <summary>
        /// Occurs when the chart's points have been modified (does not occur while dragging)
        /// </summary>
        public event EventHandler PointsChanged;
        /// <summary>
        /// Invokes the PointsChanged event. Used to notify others when they should pull point data
        /// </summary>
        private void InvokePointsChanged()
        {
            PointsChanged?.Invoke(this, new EventArgs());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
