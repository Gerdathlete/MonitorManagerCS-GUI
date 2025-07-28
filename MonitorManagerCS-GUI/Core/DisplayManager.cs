using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Navigation;

namespace MonitorManagerCS_GUI.Core
{
    public class DisplayManager
    {
        [JsonProperty]
        public DisplayInfo Display { get; }
        [JsonProperty]
        public List<VCPCodeController> VCPCodeControllers { get; set; }

        [JsonConstructor]
        public DisplayManager(DisplayInfo display, List<VCPCodeController> vcpCodeControllers)
        {
            Display = display;
            VCPCodeControllers = vcpCodeControllers;
        }

        public DisplayManager(DisplayInfo display, List<VCPCode> vcpCodes)
        {
            Display = display;
            VCPCodeControllers = MakeVCPCodeControllers(vcpCodes);
        }

        private List<VCPCodeController> MakeVCPCodeControllers(List<VCPCode> vcpCodes)
        {
            var writableCodes = vcpCodes.Where(vcpCode => vcpCode.IsWritable).ToList();

            var codeControllers = new List<VCPCodeController>();
            foreach (var vcpCode in writableCodes)
            {
                var codeController = new VCPCodeController(vcpCode);
                codeControllers.Add(codeController);
            }

            return codeControllers;
        }

        public void Save()
        {
            var fileName = Display.ConfigFileName;
            var filePath = Path.Combine(Folders.Config, fileName);

            if (!Directory.Exists(Folders.Config))
            {
                Directory.CreateDirectory(Folders.Config);
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static DisplayManager Load(DisplayInfo display)
        {
            var fileName = display.ConfigFileName;
            var filePath = Path.Combine(Folders.Config, fileName);

            if (!File.Exists(filePath)) { return null; }

            string json = File.ReadAllText(filePath);
            var displayManager = JsonConvert.DeserializeObject<DisplayManager>(json);

            displayManager.Display.NumberID = display.NumberID;

            return displayManager;
        }
    }
}
