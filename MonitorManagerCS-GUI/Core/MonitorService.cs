using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private Action _service;
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
            _service = () =>
            {
                ServiceLoop(DisplayManagers, _cancellationTokenSource.Token, UpdatePeriodMillis);
            };

            _serviceTask = Task.Run(_service);
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

            Debug.WriteLine("Shutting down monitor service...");
            _cancellationTokenSource.Cancel();
            await _serviceTask;
            return true;

            void DebugLogFailure(string reason)
            {
                Debug.WriteLine($"Failed to end monitor service: {reason}");
            }
        }

        private static void ServiceLoop(List<DisplayManager> displayManagers,
            CancellationToken cancellationToken, int updatePeriodMillis)
        {
            Debug.WriteLine("Monitor service started");

            Dictionary<DisplayManager, List<VCPCodeController>> activeControllersForDisplay;

            while (!cancellationToken.IsCancellationRequested)
            {
                activeControllersForDisplay = GetActiveControllersForDisplays(displayManagers);

                foreach (DisplayManager displayManager in displayManagers)
                {
                    foreach (var vcpController in activeControllersForDisplay[displayManager])
                    {
                        var timedValues = vcpController.TimedValues;

                        if (!timedValues.Any()) { continue; }

                        double currentHour = DateTime.Now.TimeOfDay.TotalHours;
                        int value = GetInterpolatedInt(timedValues, currentHour);

                        SetVCPValue(displayManager.Display, vcpController, value);
                    }
                }

                Thread.Sleep(updatePeriodMillis);
            }

            Debug.WriteLine("Monitor service ended");
        }

        private static void SetVCPValue(DisplayInfo Display, VCPCodeController vcpController, int value)
        {
            Debug.WriteLine($"Setting VCP code {vcpController.Code} to {value} for {Display.LongID} ({Display.NumberID})");

            Task.Run(() =>
            {
                Programs.RunProgram(Programs.controlMyMonitor,
                    $"/SetValueIfNeeded {Display.NumberID} {vcpController.Code} {value}");
            });
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
                    .Where(vcc => vcc.IsActive)
                    .ToList();

                activeControllersForDisplay.Add(displayManager, activeControllers);
            }

            return activeControllersForDisplay;
        }
    }
}
