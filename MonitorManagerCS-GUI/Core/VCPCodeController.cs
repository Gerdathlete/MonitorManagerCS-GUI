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
        public List<TimedValue> TimedValues { get; set; } = [];
        public bool IsActive { get; set; } = false;

        public VCPCodeController(VCPCode vcpCode)
        {
            ArgumentNullException.ThrowIfNull(vcpCode);

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
                return [];
            }

            return [.. vcpCode.PossibleValues
                .Split(',')
                .Select(s => s.Trim())
                .Select(int.Parse)];
        }
    }
}
