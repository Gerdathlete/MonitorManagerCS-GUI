using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Events;
using MonitorManagerCS_GUI.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MonitorManagerCS_GUI.ViewModels
{
    public class VCPCodeChart : TimeChartDraggable
    {
        private VCPCodeController _vcpController;
        public VCPCodeController VCPController
        {
            get => _vcpController;
            set => _vcpController = value;
        }
        private bool _enabled = false;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    this._vcpController.IsActive = value;
                    OnPropertyChanged(nameof(Enabled));
                }
            }
        }

        private readonly List<double> _possibleValues;

        public VCPCodeChart(VCPCodeController vcpController)
        {
            VCPController = vcpController;
            YAxis.Name = vcpController.Name;

            if (vcpController.PossibleValues.Count > 0)
            {
                _possibleValues = vcpController.PossibleValues.Select(x => (double)x).ToList();
                YAxis.CustomSeparators = _possibleValues;
                YAxis.MaxLimit = _possibleValues.Max();
                YAxis.MinLimit = _possibleValues.Min();
            }
            else
            {
                YAxis.MaxLimit = vcpController.MaximumValue;
            }

            //Points.CollectionChanged += OnPointsChanged;
            //PointModified += OnPointChanged;
        }

        //private void OnPointsChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    var controlValues = _vcpController.TimedValues;

        //    switch (e.Action)
        //    {
        //        case NotifyCollectionChangedAction.Add:
        //            if (e.NewItems is null) return;

        //            AddTimedValue();

        //            break;
        //        case NotifyCollectionChangedAction.Remove:
        //            if (e.OldItems is null) return;

        //            RemoveTimedValue();

        //            break;
        //        case NotifyCollectionChangedAction.Replace:
        //            if (e.NewItems is null) return;

        //            ReplaceTimedValue();

        //            break;
        //        case NotifyCollectionChangedAction.Move:
        //            if (e.OldItems is null) return;

        //            MoveTimedValues();

        //            break;
        //        case NotifyCollectionChangedAction.Reset:

        //            ResetTimedValues();

        //            break;
        //    }

        //    void AddTimedValue()
        //    {
        //        foreach (ObservablePoint point in e.NewItems)
        //        {
        //            var timedValue = new TimedValue(point.X, point.Y);
        //            controlValues.Insert(e.NewStartingIndex, timedValue);
        //        }
        //    }

        //    void RemoveTimedValue()
        //    {
        //        for (int i = 0; i < e.OldItems.Count; i++)
        //        {
        //            controlValues.RemoveAt(e.OldStartingIndex);
        //        }
        //    }

        //    void ReplaceTimedValue()
        //    {
        //        if (e.NewItems.Count > 1)
        //        {
        //            //This code is designed to only handle individual replace commands
        //            throw new InvalidOperationException("More than one point was replaced at once!");
        //        }

        //        var point = (ObservablePoint)e.NewItems[0];
        //        var timedValue = new TimedValue(point.X, point.Y);

        //        controlValues[e.NewStartingIndex] = timedValue;
        //    }

        //    void MoveTimedValues()
        //    {
        //        var movedItem = controlValues[e.OldStartingIndex];
        //        controlValues.Insert(e.NewStartingIndex, movedItem);
        //    }

        //    void ResetTimedValues()
        //    {
        //        controlValues = new List<TimedValue>();

        //        var points = (ObservableCollection<ObservablePoint>)sender;

        //        foreach (var point in points)
        //        {
        //            var timedValue = new TimedValue(point.X, point.Y);
        //            controlValues.Add(timedValue);
        //        }
        //    }

        //    CheckTimedValues();
        //}

        //private void OnPointChanged(object sender, IndexedPropertyChangedArgs e)
        //{
        //    var point = (ObservablePoint)sender;

        //    var timedValue = _vcpController.TimedValues[e.Index];

        //    if (e.PropertyName == nameof(ObservablePoint.X))
        //    {
        //        timedValue.Hour = point.X;
        //    }

        //    if (e.PropertyName == nameof(ObservablePoint.Y))
        //    {
        //        timedValue.Value = point.Y;
        //    }

        //    CheckTimedValues(point, timedValue, e.PropertyName);
        //}

        public override void OnMousePressed(PointerCommandArgs args)
        {
            base.OnMousePressed(args);

            var originalArgs = (MouseButtonEventArgs)args.OriginalEventArgs;

            if (originalArgs.ChangedButton == MouseButton.Right)
            {

            }
        }

        public override void OnMouseReleased(PointerCommandArgs args)
        {
            base.OnMouseReleased(args);

            _vcpController.TimedValues = GetTimedValues(Points);
        }

        private List<TimedValue> GetTimedValues(ObservableCollection<ObservablePoint> points)
        {
            
        }

        private void CheckTimedValues(ObservablePoint point, TimedValue timedValue, 
            string propertyName)
        {
            CheckTimedValues();

            if (propertyName == nameof(ObservablePoint.X) && point.X != timedValue.Hour)
            {
                ThrowException();
            }

            if (propertyName == nameof(ObservablePoint.Y) && point.Y != timedValue.Value)
            {
                ThrowException();
            }

            void ThrowException()
            {
                throw new Exception("TimedValue was set improperly!");
            }
        }
        private void CheckTimedValues()
        {
            StringBuilder debugMessage = new StringBuilder();
            debugMessage.Append("TimedValues = {");

            int i = 0;
            int indexOfLast = _vcpController.TimedValues.Count - 1;
            foreach (var value in _vcpController.TimedValues)
            {
                EnsureValidValue(value);
                debugMessage.Append(value.ToString());

                if (i < indexOfLast)
                {
                    debugMessage.Append(", ");
                }

                i++;
            }
            debugMessage.Append("}");

            Debug.WriteLine(debugMessage);

            void EnsureValidValue(TimedValue timedValue)
            {
                var value = (double)timedValue.Value;
                if (i == 0 || i == indexOfLast) return;
                
                //If the value isn't really close to an integer
                if (value - Math.Round(value) > 1e-10)
                {
                    Debug.WriteLine("Invalid TimeValue detected!");
                    throw new Exception("TimedValue has invalid value!");
                }
            }
        }

        public override LvcPointD? GetValidPointLocation(LvcPointD chartPos, ObservablePoint point = null)
        {
            LvcPointD? baseOutput = base.GetValidPointLocation(chartPos, point);
            if (_possibleValues is null || baseOutput is null) { return baseOutput; }

            LvcPointD unsnappedPoint = baseOutput.Value;

            return new LvcPointD(
                unsnappedPoint.X,
                RoundToNearest(unsnappedPoint.Y, _possibleValues));
        }

        public static double RoundToNearest(double value, List<double> possibleValues)
        {
            return possibleValues
                .OrderBy(v => Math.Abs(v - value))
                .ThenByDescending(v => v)
                .First();
        }
    }
}
