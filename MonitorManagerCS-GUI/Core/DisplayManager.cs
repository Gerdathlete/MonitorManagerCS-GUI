using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MonitorManagerCS_GUI.Core
{
    public class DisplayManager
    {
        [JsonProperty]
        public DisplayInfo Display { get; set; }
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

        public static List<VCPCodeController> MakeVCPCodeControllers(List<VCPCode> vcpCodes)
        {
            var codeControllers = new List<VCPCodeController>();
            foreach (var vcpCode in vcpCodes)
            {
                var codeController = new VCPCodeController(vcpCode);
                codeControllers.Add(codeController);
            }

            return codeControllers;
        }

        public void Save(string subFolder = null)
        {
            var fileName = Display.ConfigFileName;
            var fileDirectory = Folders.Config;

            if (subFolder != null)
            {
                fileDirectory = Path.Combine(fileDirectory, subFolder);
            }

            var filePath = Path.Combine(fileDirectory, fileName);

            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
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

            displayManager.Display = display;

            return displayManager;
        }

        public static async Task<DisplayManager> ConvertOldConfig(DisplayInfo display)
        {
            var displayManager = Load(display);

            if (displayManager == null)
            {
                Debug.WriteLine($"{display} doesn't have an existing or valid config file.");
            }

            var newVCPCodes = await DisplayRetriever.GetVCPCodes(display);
            var newVCPControllers = MakeVCPCodeControllers(newVCPCodes);

            bool madeBackup = false;
            foreach (var oldVCPController in displayManager.VCPCodeControllers)
            {
                if (!oldVCPController.IsActive) continue;

                var newVCPController = newVCPControllers
                    .Where(vcp => vcp.Code == oldVCPController.Code).FirstOrDefault();

                if (newVCPController == null)
                {
                    Debug.WriteLine($"Active VCP code {oldVCPController.Name} " +
                        $"({oldVCPController.Code}) is no longer supported.");

                    if (!madeBackup)
                    {
                        Debug.WriteLine($"Saving a backup of this display's config");
                        displayManager.Save("Old");
                    }

                    continue;
                }

                newVCPController.TimedValues = oldVCPController.TimedValues;
                newVCPController.IsActive = true;
            }

            displayManager.VCPCodeControllers = newVCPControllers;
            displayManager.Save();

            return displayManager;
        }
    }
}
