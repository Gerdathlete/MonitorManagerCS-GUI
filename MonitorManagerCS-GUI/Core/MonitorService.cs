using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorManagerCS_GUI.Core
{
    sealed class MonitorService
    {
        private static MonitorService _instance;
        private static readonly object _lock = new object();

        private MonitorService() { }

        public static MonitorService Instance()
        {
            if (_instance is null)
            {
                lock (_lock)
                {
                    if (_instance is null)
                    {
                        _instance = new MonitorService();
                    }
                }
            }

            return _instance;
        }

        public List<DisplayManager> DisplayManagers { get; set; }
        public int UpdatePeriodMillis { get; set; } = 60000;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _serviceTask;

        /// <summary>
        /// Restarts the monitor service.
        /// </summary>
        /// <returns></returns>
        public async Task Restart()
        {
            await End();
            Start();
        }

        /// <summary>
        /// Starts the monitor service. Returns true if the service was started.
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            if (DisplayManagers is null || !DisplayManagers.Any())
            {
                DebugLogFailure("DisplayManagers is null or empty!");
                return false;
            }

            if (_serviceTask != null && !_serviceTask.IsCompleted)
            {
                DebugLogFailure("Monitor service is already running!");
                return false;
            }

            Debug.WriteLine("Starting monitor service...");

            _cancellationTokenSource = new CancellationTokenSource();

            _serviceTask = Task.Run(async () =>
            {
                await ServiceLoop(DisplayManagers, _cancellationTokenSource.Token, UpdatePeriodMillis);
            });

            return true;

            void DebugLogFailure(string reason)
            {
                Debug.WriteLine($"Failed to start monitor service: {reason}");
            }
        }

        /// <summary>
        /// Ends the monitor service. Returns true if the service was ended.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> End()
        {
            if (_serviceTask is null || _serviceTask.IsCompleted)
            {
                DebugLogFailure("Service is not running!");
                return false;
            }

            try
            {
                Debug.WriteLine("Shutting down monitor service...");
                _cancellationTokenSource.Cancel();
                await _serviceTask;
                return true;
            }
            catch (OperationCanceledException)
            {
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Monitor service shutdown error: {ex}");
                return false;
            }

            void DebugLogFailure(string reason)
            {
                Debug.WriteLine($"Failed to end monitor service: {reason}");
            }
        }

        private static async Task ServiceLoop(List<DisplayManager> displayManagers,
            CancellationToken cancellationToken, int updatePeriodMillis)
        {
            DebugLog("Monitor service started");

            while (!cancellationToken.IsCancellationRequested)
            {
                await UpdateActiveVCPCodes(displayManagers);

                Thread.Sleep(updatePeriodMillis);
                try
                {
                    await Task.Delay(updatePeriodMillis, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            DebugLog("Monitor service ended");
        }

        public async Task UpdateActiveVCPCodes() => await UpdateActiveVCPCodes(DisplayManagers);
        private static async Task UpdateActiveVCPCodes(List<DisplayManager> displayManagers)
        {
            DebugLog("Started updated VCP codes.");

            var activeControllersForDisplay = GetActiveControllersForDisplays(displayManagers);
            var VCPCommands = new StringBuilder();

            foreach (DisplayManager displayManager in displayManagers)
            {
                foreach (var vcpController in activeControllersForDisplay[displayManager])
                {
                    var timedValues = vcpController.TimedValues;

                    double currentHour = DateTime.Now.TimeOfDay.TotalHours;
                    int value = GetInterpolatedInt(timedValues, currentHour);

                    if (VCPCommands.Length > 0)
                    {
                        VCPCommands.Append(' ');
                    }

                    VCPCommands.Append(
                        VCPSetValueCommand(displayManager.Display, vcpController, value));
                    LogVCPChange(vcpController.Code, value, displayManager.Display);
                }
            }

            await Programs.RunProgramAsync(Programs.ControlMyMonitor, VCPCommands.ToString());

            DebugLog("Finished updating VCP codes.");
        }

        private static void LogVCPChange(string code, int value, DisplayInfo display)
        {
            DebugLog($"Setting VCP code {code} to {value}" +
                $" for {display.LongID} ({display.NumberID})");
        }

        private static string VCPSetValueCommand(DisplayInfo display, 
            VCPCodeController vcpController, int value)
        {
            return $"/SetValueIfNeeded {display.NumberID} {vcpController.Code} {value}";
        }

        private static int GetInterpolatedInt(ICollection<TimedValue> timedValues, double hour)
        {
            //Handle edge cases
            if (!timedValues.Any())
            {
                throw new ArgumentException($"{nameof(timedValues)} must contain at least one item!");
            }

            if (timedValues.Count == 1)
            {
                return DoubleToInt(timedValues.First().Value);
            }

            //Just in case, check for an exact match
            var matchingTimedValue = timedValues.Where(tv => tv.Hour == hour).FirstOrDefault();
            if (matchingTimedValue != null)
            {
                return DoubleToInt(matchingTimedValue.Value);
            }

            var prevTimedValue = timedValues.Where(tv => tv.Hour < hour).LastOrDefault();
            var nextTimedValue = timedValues.Where(tv => tv.Hour > hour).FirstOrDefault();

            double? prevHour = null;
            double? nextHour = null;
            double? prevValue = null;
            double? nextValue = null;
            if (prevTimedValue is null)
            {
                var lastTimedValue = timedValues.Last();
                prevHour = lastTimedValue.Hour - 24;
                prevValue = lastTimedValue.Value;
            }
            else
            {
                prevHour = prevTimedValue.Hour;
                prevValue = prevTimedValue.Value;
            }

            if (nextTimedValue is null)
            {
                var firstTimedValue = timedValues.First();
                nextHour = firstTimedValue.Hour + 24;
                nextValue = firstTimedValue.Value;
            }
            else
            {
                nextHour = nextTimedValue.Hour;
                nextValue = nextTimedValue.Value;
            }

            var periodLength = (nextHour - prevHour);
            var timeSincePeriodBegan = (hour - prevHour);

            //Stop divide by zero case
            if (periodLength == 0)
            {
                return DoubleToInt(nextValue);
            }

            var periodPercentage = timeSincePeriodBegan / periodLength;
            var valueDiff = (nextValue - prevValue);
            var interpolatedValue = (prevValue + valueDiff * periodPercentage);
            return DoubleToInt(interpolatedValue);
        }

        private static int DoubleToInt(double? value)
        {
            if (value == null) { return 0; }
            return (int)Math.Round((double)value);
        }

        private static Dictionary<DisplayManager, List<VCPCodeController>>
            GetActiveControllersForDisplays(List<DisplayManager> displayManagers)
        {
            var activeControllersForDisplay =
                    new Dictionary<DisplayManager, List<VCPCodeController>>();
            foreach (DisplayManager displayManager in displayManagers)
            {
                var activeControllers = new List<VCPCodeController>();
                activeControllers = displayManager.VCPCodeControllers
                    .Where(vcp => vcp.IsActive && vcp.TimedValues.Any())
                    .ToList();

                activeControllersForDisplay.Add(displayManager, activeControllers);
            }

            return activeControllersForDisplay;
        }

        private static void DebugLog(string message)
        {
            Debug.WriteLine("[Monitor Service] " + message);
        }
    }
}
