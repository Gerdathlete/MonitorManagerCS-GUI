using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using MonitorManagerCS_GUI.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonitorManagerCS_GUI.ViewModels
{
    public class VCPCodeChart : TimeChartDraggable
    {
        public VCPCodeControl VCPCode { get; set; }
        public bool Enabled { get; set; } = true;

        private List<double> _possibleValues;

        public VCPCodeChart(VCPCodeControl vcpCode)
        {
            VCPCode = vcpCode;
            YAxis.Name = vcpCode.Name;

            if (vcpCode.PossibleValues.Count > 0)
            {
                _possibleValues = vcpCode.PossibleValues.Select(x => (double)x).ToList();
                YAxis.CustomSeparators = _possibleValues;
                YAxis.MaxLimit = _possibleValues.Max();
            }
            else
            {
                YAxis.MaxLimit = vcpCode.MaximumValue;
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
