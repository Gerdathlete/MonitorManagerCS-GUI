using System.Text.Json.Serialization;

namespace MonitorManagerCS_GUI
{
    public class VCPCode
    {
        [JsonPropertyName("VCP Code")]
        public string Code { get; set; }

        [JsonPropertyName("VCP Code Name")]
        public string Name { get; set; }

        [JsonPropertyName("Read-Write")]
        public string ReadWrite { get; set; }

        [JsonPropertyName("Current Value")]
        public string CurrentValue { get; set; }

        [JsonPropertyName("Maximum Value")]
        public string MaximumValue { get; set; }

        [JsonPropertyName("Possible Values")]
        public string PossibleValues { get; set; }
    }
}
