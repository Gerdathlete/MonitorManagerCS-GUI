﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MonitorManagerCS_GUI.Core
{
    public class VCPCodeControl
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int? CurrentValue { get; set; }
        public int? MaximumValue { get; set; }
        public List<int> PossibleValues { get; set; }
        public List<TimedValue> TimedValues { get; set; } = new List<TimedValue>();
        public bool IsActive { get; set; } = false;

        public VCPCodeControl(VCPCode vcpCode)
        {
            if (vcpCode is null) { throw new ArgumentNullException(nameof(vcpCode)); }

            Code = vcpCode.Code;
            Name = vcpCode.Name;
            CurrentValue = ParseNullableInt(vcpCode.CurrentValue);
            MaximumValue = ParseNullableInt(vcpCode.MaximumValue);
            PossibleValues = GetPossibleValues(vcpCode);
        }

        private int? ParseNullableInt(string intString)
        {
            if (string.IsNullOrEmpty(intString)) { return null; }

            return int.Parse(intString);
        }

        private List<int> GetPossibleValues(VCPCode vcpCode)
        {
            var possibleValuesStr = vcpCode.PossibleValues;
            if (string.IsNullOrEmpty(possibleValuesStr))
            {
                return new List<int>();
            }

            return vcpCode.PossibleValues
                .Split(',')
                .Select(s => s.Trim())
                .Select(int.Parse)
                .ToList();
        }
    }

    public struct TimedValue
    {
        internal double? Hour { get; set; }
        internal double? Value { get; set; }
    }
}
