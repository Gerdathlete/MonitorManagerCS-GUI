using System;
using System.Collections.Generic;
using System.Linq;

namespace MonitorManagerCS_GUI.Core
{
    public class VCPCodeController
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int? CurrentValue { get; set; }
        public int? MaximumValue { get; set; }
        public List<int> PossibleValues { get; set; }
        public List<TimedValue> TimedValues { get; set; } = new List<TimedValue>();
        public bool IsActive { get; set; } = false;

        public VCPCodeController(VCPCode vcpCode)
        {
            if (vcpCode is null) { throw new ArgumentNullException(nameof(vcpCode)); }

            Code = vcpCode.Code;
            Name = vcpCode.Name;
            CurrentValue = ParseNullableInt(vcpCode.CurrentValue);
            MaximumValue = ParseNullableInt(vcpCode.MaximumValue);
            PossibleValues = GetPossibleValues(vcpCode);
        }
        public VCPCodeController() { }

        private static int? ParseNullableInt(string intString)
        {
            if (string.IsNullOrEmpty(intString)) { return null; }

            return int.Parse(intString);
        }

        private static List<int> GetPossibleValues(VCPCode vcpCode)
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
}
