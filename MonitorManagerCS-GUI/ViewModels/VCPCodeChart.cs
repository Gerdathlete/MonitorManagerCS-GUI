using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using MonitorManagerCS_GUI.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

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
        private bool _enabled;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
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

            LoadTimedValues(vcpController.TimedValues);

            Enabled = vcpController.IsActive;

            PointsChanged += OnPointsChanged;
        }

        private void OnPointsChanged(object sender, EventArgs e)
        {
            //UpdateVCPValues();
            //CheckTimedValues(); //Enable if debugging
        }

        private void LoadTimedValues(List<TimedValue> timedValues)
        {
            var points = new List<ObservablePoint>();
            foreach (var timedValue in timedValues)
            {
                var point = new ObservablePoint(timedValue.Hour, timedValue.Value);
                points.Add(point);
            }

            Points = new ObservableCollection<ObservablePoint>(points);
            UpdateWrappingPoints();
        }

        public void UpdateVCPController()
        {
            _vcpController.IsActive = _enabled;
            _vcpController.TimedValues = GetTimedValues(Points);
        }

        /// <summary>
        /// Creates a new list of TimedValues based on the current location of the chart points
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private List<TimedValue> GetTimedValues(ObservableCollection<ObservablePoint> points)
        {
            var values = new List<TimedValue>();

            foreach (var point in points)
            {
                bool isWithinBounds = point.X >= TimeAxis.MinLimit && point.X <= TimeAxis.MaxLimit;
                if (isWithinBounds)
                {
                    var timedValue = new TimedValue(point.X, point.Y);
                    values.Add(timedValue);
                }
            }

            return values;
        }

        /// <summary>
        /// Prints information about the vcp controller's TimedValues for debug purposes
        /// </summary>
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

                //If the value isn't really close to an integer, let the user know
                if (value - Math.Round(value) > 1e-10)
                {
                    Debug.WriteLine("Invalid TimeValue detected!");
                }
            }
        }

        public override LvcPointD? GetValidPointLocation(LvcPointD chartPos,
            ObservablePoint point = null)
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
