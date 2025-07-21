using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace MonitorManagerCS_GUI
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
                    var sortedPoints = new ObservableCollection<ObservablePoint>(value.OrderBy(p => p.X));
                    if (_points != sortedPoints)
                    {
                        _points = sortedPoints;
                        OnPropertyChanged(nameof(Points));
                    }
                }
            }
        }
        public ISeries[] Series { get; }
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
        private TooltipPosition _prevTooltipPos;

        public TimeChartDraggable()
        {
            PointerReleasedCommand = new RelayCommand<PointerCommandArgs>(OnMouseReleased);
            PointerMovedCommand = new RelayCommand<PointerCommandArgs>(OnMouseMoved);
            PointerPressedCommand = new RelayCommand<PointerCommandArgs>(OnMousePressed);

            _points = new ObservableCollection<ObservablePoint>();

            var lineSeries = new LineSeries<ObservablePoint>
            {
                Values = Points,
                GeometrySize = 10,
                LineSmoothness = 0,
            };

            Series = new ISeries[] { lineSeries };

            TimeAxis = new Axis
            {
                Name = "Time",
                MinLimit = 0,
                MaxLimit = 24,
                Labeler = v => DataFormatter.GetReadableTime(v)
            };

            YAxis = new Axis
            {
                Name = "Brightness",
                MinLimit = 0,
                MaxLimit = 100,
            };

            XAxes = new[] { TimeAxis };

            YAxes = new[] { YAxis };

            _prevTooltipPos = TooltipPos;
        }

        /// <summary>
        /// Runs when a mouse button is pressed. Handles dragging points and adding new points.
        /// </summary>
        /// <param name="args"></param>
        private void OnMousePressed(PointerCommandArgs args)
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
                }
            }
        }

        /// <summary>
        /// Runs when the mouse is moved. Updates the position of points when they are dragged.
        /// </summary>
        /// <param name="args"></param>
        private void OnMouseMoved(PointerCommandArgs args)
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

            
        }

        /// <summary>
        /// Runs when a mouse button is released. Releases dragged points.
        /// </summary>
        /// <param name="args"></param>
        private void OnMouseReleased(PointerCommandArgs args)
        {
            ShowTooltips();
            

            if (_draggedPoint == null) return;

            _draggedPoint = null;
        }

        private LvcPointD? GetValidPointLocation(LvcPointD chartPos, ObservablePoint point = null)
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

        /// <summary>
        /// Adds a point to the chart at the correct index so that the lines connect in order of X value
        /// </summary>
        /// <param name="chartPos"></param>
        public ObservablePoint AddPoint(LvcPointD chartPos) => AddPoint(chartPos.X, chartPos.Y);
        /// <summary>
        /// Adds a point to the chart at the correct index so that the lines connect in order of X value
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
