using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace MonitorManagerCS_GUI
{
    public partial class MainWindow : Window
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenuStrip;
        private ToolStripMenuItem showTrayMenuItem;
        private ToolStripMenuItem exitTrayMenuItem;
        public MainViewModel ViewModel { get; set; }
        public StartupSettings startupSettings;
        public Settings settings;
        public Task monitorServiceTask;
        private CancellationTokenSource monitorServiceTokenSource;
        private static readonly string[] statusPrefixes = { "", "  ", "    " };
        private byte statusPrefixIndex;

        /// <summary>
        /// Entry point of the program.
        /// </summary>
        public MainWindow()
        {
            //Mandatory line to make the main window work
            InitializeComponent();

            InitializeTrayIcon();

            //Run the MainWindow_StateChanged function when the window's state changes (e.g. when the window is opened, minimized, or closed)
            StateChanged += MainWindow_StateChanged;

            //MinimizeToTray();

            ViewModel = new MainViewModel();
            DataContext = ViewModel;



            ViewModel.Tab_Display1.Chart.AddPoint(3, 51);
        }

        private void StartMonitorService()
        {
            //End the monitor service (if its running)
            EndMonitorService();

            //Create a new token source for the service (this allows it to be ended)
            monitorServiceTokenSource = new CancellationTokenSource();

            //Launch the monitor service
            Debug.WriteLine("Launching monitor service...");
            monitorServiceTask = Task.Run(() => { App.MonitorService(settings, monitorServiceTokenSource.Token); });
        }

        private void EndMonitorService()
        {
            if (monitorServiceTask != null && !monitorServiceTask.IsCompleted)
            {
                monitorServiceTokenSource.Cancel();
                Debug.WriteLine("Shutting down monitor service...");
                monitorServiceTask.Wait();
            }
        }

        private void InitializeTrayIcon()
        {
            trayIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon("icon.ico"),
                Visible = false,
                Text = "Nathan\'s Monitor Manager" // Tooltip text
            };

            trayMenuStrip = new ContextMenuStrip();
            showTrayMenuItem = new ToolStripMenuItem("Show");
            exitTrayMenuItem = new ToolStripMenuItem("Exit");

            showTrayMenuItem.Click += ShowTrayMenuItem_Click;
            exitTrayMenuItem.Click += ExitTrayMenuItem_Click;

            trayMenuStrip.Items.Add(showTrayMenuItem);
            trayMenuStrip.Items.Add(exitTrayMenuItem);

            trayIcon.ContextMenuStrip = trayMenuStrip;

            trayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    Show();
                    WindowState = WindowState.Normal;
                    trayIcon.Visible = false;
                }
            };
        }

        private void ShowTrayMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            trayIcon.Visible = false;
        }

        private void ExitTrayMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                MinimizeToTray();
            }
        }

        private void MinimizeToTray()
        {
            Hide();
            trayIcon.Visible = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            trayIcon.Dispose();
        }

        public string GetSettingsPathFromUser()
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Filter = "JSON Files|*.json",
                Title = "Select a Monitor Manager settings file",
                InitialDirectory = StartupSettings.DefaultSettingsFilePath
            };

            //Stop the user from selecting the startup settings file
            fileDialog.FileOk += (sender, e) =>
            {
                if (fileDialog.FileName == StartupSettings.StartupSettingsFilePath)
                {
                    e.Cancel = true;
                    System.Windows.MessageBox.Show($"The file path '{StartupSettings.StartupSettingsFilePath}' is reserved for startup settings.", "Invalid Filename", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            DialogResult diaResult = fileDialog.ShowDialog();

            if (diaResult == System.Windows.Forms.DialogResult.OK)
            {
                return fileDialog.FileName;
            }
            Debug.WriteLine("User did not select a file.");
            return null;
        }

        private string StatusPrefix()
        {
            string output = statusPrefixes[statusPrefixIndex++];
            if (statusPrefixIndex >= statusPrefixes.Length) statusPrefixIndex = 0;
            return output;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            EndMonitorService();
        }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TabViewModel> Tabs { get; set; }
        public TabViewModel SelectedTab { get; set; }
        public DisplayTab Tab_Display1;
        public SettingsTab Tab_Settings;

        public MainViewModel()
        {
            Tab_Display1 = new DisplayTab
            {
                TabName = "Display 1"
            };

            Tab_Settings = new SettingsTab
            {
                TabName = "Settings",
                Text = "This is a settings tab."
            };

            Tabs = new ObservableCollection<TabViewModel>
            {
                Tab_Display1,
                Tab_Settings
            };

            SelectedTab = Tab_Display1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TabViewModel
    {
        public string TabName { get; set; }
    }

    public class DisplayTab : TabViewModel
    {
        public ObservableCollection<VCPCode> VCPCodes { get; set; }
        public VCPCode SelectedVCPCode { get; set; }
        public TimeChartDraggable Chart { get; set; }
        public DisplayTab()
        {
            VCPCodes = new ObservableCollection<VCPCode>();
            Chart = new TimeChartDraggable();
        }
    }

    public class SettingsTab : TabViewModel
    {
        public string Text { get; set; }
    }

    public partial class TimeChartDraggable : INotifyPropertyChanged
    {
        private ObservableCollection<ObservablePoint> _points;
        public ObservableCollection<ObservablePoint> Points
        {
            get => _points;
            set
            {
                if (_points != value)
                {
                    var sortedPoints = new ObservableCollection<ObservablePoint>(value.OrderBy(p => p.X));
                    if (_points != sortedPoints)
                    {
                        _points = sortedPoints;
                        OnPropertyChanged(nameof(Points));
                    }
                }
            }
        }
        public ISeries[] Series { get; }
        public Axis TimeAxis { get; }
        public Axis YAxis { get; set; }
        public Axis[] XAxes { get; }
        public Axis[] YAxes { get; }
        public double XSnap { get; set; } = .25;
        public double YSnap { get; set; } = 1;
        public IRelayCommand<PointerCommandArgs> PointerReleasedCommand { get; }
        public IRelayCommand<PointerCommandArgs> PointerMovedCommand { get; }
        public IRelayCommand<PointerCommandArgs> PointerPressedCommand { get; }

        private ObservablePoint _draggedPoint = null;

        public TimeChartDraggable()
        {
            PointerReleasedCommand = new RelayCommand<PointerCommandArgs>(OnMouseReleased);
            PointerMovedCommand = new RelayCommand<PointerCommandArgs>(OnMouseMoved);
            PointerPressedCommand = new RelayCommand<PointerCommandArgs>(OnMousePressed);

            _points = new ObservableCollection<ObservablePoint>
            {
                new ObservablePoint(8, 20),
                new ObservablePoint(12, 80),
                new ObservablePoint(18, 30)
            };

            var lineSeries = new LineSeries<ObservablePoint>
            {
                Values = Points,
                GeometrySize = 10,
                LineSmoothness = 0,
            };

            lineSeries.ChartPointPointerDown += OnClickPoint;

            Series = new ISeries[] { lineSeries };

            TimeAxis = new Axis
            {
                Name = "Time",
                MinLimit = 0,
                MaxLimit = 24,
                Labeler = v => DataFormatter.GetReadableTime(v)
            };

            YAxis = new Axis
            {
                Name = "Brightness",
                MinLimit = 0,
                MaxLimit = 100,
            };

            XAxes = new[] { TimeAxis };

            YAxes = new[] { YAxis };
        }

        private void OnMousePressed(PointerCommandArgs args)
        {
            var chart = (ICartesianChartView)args.Chart;
            var mousePos = args.PointerPosition;

            var clickedPoints = chart.GetPointsAt(mousePos);

            if (clickedPoints.Any()) { return; }

            var mouseChartPos = chart.ScalePixelsToData(mousePos);

            var newPoint = AddPoint(mouseChartPos);
            _draggedPoint = newPoint;
        }

        /// <summary>
        /// Runs when a mouse button is pressed. Handles the initiation of dragging points.
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="point"></param>
        private void OnClickPoint(IChartView chart, ChartPoint<ObservablePoint, CircleGeometry, LabelGeometry> point)
        {
            if (point == null) return;

            Debug.WriteLine($"Clicked on {point.Coordinate}");
            _draggedPoint = point.Model;
        }

        /// <summary>
        /// Runs when the mouse is moved. Updates the position of points when they are dragged.
        /// </summary>
        /// <param name="args"></param>
        private void OnMouseMoved(PointerCommandArgs args)
        {
            if (_draggedPoint == null) return;

            //Move the point to the mouse position with snapping
            var mousePos = args.PointerPosition;
            var chart = (ICartesianChartView)args.Chart;
            var mouseChartPos = chart.ScalePixelsToData(mousePos);

            var newPosNullable = GetValidPointLocation(mouseChartPos, _draggedPoint);
            if (newPosNullable == null) return;

            var newPos = (LvcPointD)newPosNullable;

            _draggedPoint.X = newPos.X;
            _draggedPoint.Y = newPos.Y;

            //Reorder points if needed
            UpdatePointIndex(_draggedPoint);
        }

        /// <summary>
        /// Runs when a mouse button is released. Releases dragged points.
        /// </summary>
        /// <param name="args"></param>
        private void OnMouseReleased(PointerCommandArgs args)
        {
            if (_draggedPoint == null) return;

            Debug.WriteLine($"Released point at {_draggedPoint.Coordinate}");

            _draggedPoint = null;
        }

        private LvcPointD? GetValidPointLocation(LvcPointD chartPos, ObservablePoint point = null)
        {
            LvcPointD pointLocation = new LvcPointD(chartPos.X, chartPos.Y);

            //Snap to grid
            pointLocation.X = Math.Round(pointLocation.X / XSnap) * XSnap;
            pointLocation.Y = Math.Round(pointLocation.Y / YSnap) * YSnap;

            //Prevent point locations outside of the chart boundary
            if (TimeAxis.MaxLimit != null && pointLocation.X > TimeAxis.MaxLimit)
            {
                pointLocation.X = (double)TimeAxis.MaxLimit;
            }

            if (TimeAxis.MinLimit != null && pointLocation.X < TimeAxis.MinLimit)
            {
                pointLocation.X = (double)TimeAxis.MinLimit;
            }

            if (YAxis.MaxLimit != null && pointLocation.Y > YAxis.MaxLimit)
            {
                pointLocation.Y = (double)YAxis.MaxLimit;
            }

            if (YAxis.MinLimit != null && pointLocation.Y < YAxis.MinLimit)
            {
                pointLocation.Y = (double)YAxis.MinLimit;
            }

            //Prevent the location from being at the same X-value as another point

            int oldIndex = 0;
            if (point != null)
            {
                oldIndex = (point.MetaData != null) ? point.MetaData.EntityIndex : 0;
                _points.Remove(point);
            }

            int pointIndex = GetPointIndex(pointLocation.X);

            var replacedPointX = (pointIndex < _points.Count) ? _points[pointIndex].X : null;

            int leftDist = 0;
            int rightDist = 0;

            if (pointLocation.X == replacedPointX)
            {
                //Move left by the snap distance until we reach a location without a point
                leftDist++;
                double testLocation = pointLocation.X - XSnap;
                int testIndex = pointIndex - 1;
                var testPointX = (testIndex >= 0) ? _points[testIndex].X : null;

                while (testPointX == testLocation)
                {
                    leftDist++;
                    testLocation -= XSnap;
                    testIndex--;
                    testPointX = (testIndex >= 0) ? _points[testIndex].X : null;
                }

                //Move right by the snap distance until we reach a location without a point
                rightDist++;
                testLocation = pointLocation.X + XSnap;
                testIndex = pointIndex + 1;
                testPointX = (testIndex < _points.Count) ? _points[testIndex].X : null;

                while (testPointX == testLocation)
                {
                    rightDist++;
                    testLocation += XSnap;
                    testIndex++;
                    testPointX = (testIndex < _points.Count) ? _points[testIndex].X : null;
                }

                double newX;

                //Move the shorter distance to the new location
                if (leftDist >= rightDist)
                {
                    //Try moving right, check if out of bounds
                    newX = pointLocation.X + rightDist * XSnap;
                    if (newX > TimeAxis.MaxLimit)
                    {
                        //Try moving left, return null if still out of bounds
                        newX = pointLocation.X - leftDist * XSnap;
                        if (newX < TimeAxis.MinLimit)
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    //Try moving left, check if out of bounds
                    newX = pointLocation.X - leftDist * XSnap;
                    if (newX < TimeAxis.MinLimit)
                    {
                        //Try moving right, return null if still out of bounds
                        newX = pointLocation.X + rightDist * XSnap;
                        if (newX > TimeAxis.MaxLimit)
                        {
                            return null;
                        }
                    }
                }

                pointLocation.X = newX;
            }

            if (point != null)
            {
                _points.Insert(oldIndex, point);
            }

            return pointLocation;
        }

        /// <summary>
        /// Adds a point to the chart at the correct index so that the lines connect in order of X value
        /// </summary>
        /// <param name="chartPos"></param>
        public ObservablePoint AddPoint(LvcPointD chartPos) => AddPoint(chartPos.X, chartPos.Y);
        /// <summary>
        /// Adds a point to the chart at the correct index so that the lines connect in order of X value
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public ObservablePoint AddPoint(double x, double y)
        {
            LvcPointD? pointLocNullable = GetValidPointLocation(new LvcPointD(x, y));
            if (pointLocNullable == null) return null;

            var pointLoc = (LvcPointD)pointLocNullable;

            var newPoint = new ObservablePoint(pointLoc.X, pointLoc.Y);

            int pointIndex = GetPointIndex(newPoint.X);
            _points.Insert(pointIndex, newPoint);

            return newPoint;
        }

        public void UpdatePointIndex(ObservablePoint point)
        {
            _points.Remove(point);
            int newIndex = GetPointIndex(point.X);
            _points.Insert(newIndex, point);
        }

        public int GetPointIndex(double? pointX)
        {
            Debug.Assert(pointX != null);

            int pointIndex = 0;
            while (pointIndex < _points.Count && _points[pointIndex].X < pointX)
            { pointIndex++; }

            return pointIndex;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class VCPCode
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int MaximumValue { get; set; }
        public int CurrentValue { get; set; }
    }

    public static class DataFormatter
    {
        public static string GetReadableTime(double hour)
        {
            int hourInt = (int)hour;
            int minute = (int)(hour % 1 * 60);

            return GetReadableTime(hourInt, minute);
        }
        public static string GetReadableTime(int hour, int minute)
        {
            string AMPM = hour < 12 ? "AM" : "PM";

            hour %= 12;
            hour = hour == 0 ? 12 : hour;

            string hourStr = hour.ToString();

            //Add a leading zero to the minute if needed
            string minuteStr = minute < 10 ? $"0{minute}" : minute.ToString();

            return $"{hourStr}:{minuteStr} {AMPM}";
        }

        public static string GetMinMaxString(int min, int max)
        {
            return $"{min}-{max}";
        }
    }

    public static class DataInterpreter
    {
        public static int[] ParseReadableTime(string time)
        {
            //Example input: "7:08 PM"
            //Returns [19, 8]
            byte hour = 0;
            byte minute = 0;
            string buffer = "";
            for (int i = 0; i < time.Length; i++)
            {
                char c = time[i];

                //If we reach a colon, the content of the buffer is the hour
                if (c == ':')
                {
                    //Store the buffer value as the hour
                    hour = byte.Parse(buffer);
                    //Empty the buffer, go to the next character
                    buffer = "";
                    continue;
                }

                //If we reach a space, the content of the buffer is the minute, possibly with a leading zero
                if (c == ' ')
                {
                    //If there is a leading zero (ie. buffer == "0#"), remove it
                    if (time[i - 2] == '0')
                    {
                        buffer = buffer.Substring(1);
                    }
                    //Store the buffer value as the minute
                    minute = byte.Parse(buffer);

                    //Put the last two characters into the buffer
                    buffer = time.Substring(i + 1);

                    //If its PM, but not 12 PM, add 12 to the hour
                    if (buffer == "PM")
                    {
                        if (hour != 12) hour += 12;
                    }
                    //If its 12 AM, the hour is 0. Ex: "12:03 AM" -> hour = 0
                    else if (hour == 12)
                    {
                        hour = 0;
                    }
                    //If its AM, and not 12 AM, the values are already correct

                    //We are done!
                    break;
                }

                //Add the character to the buffer
                buffer += c;
            }
            return new int[] { hour, minute };
        }

        public static Regex readableTimeRegex = new Regex(@"^([1-9]|1[0-2]):[0-5][0-9] (AM|PM)$");

        public static Regex blueLightRegex = new Regex(@"^[0-4]-[0-4]$");

        public static Regex minMaxRegex = new Regex(@"^(?:0|[1-9][0-9]?|100)-(?:0|[1-9][0-9]?|100)$");

        /// <summary>
        /// Turns a min-max string into two ints
        /// (e.g. "2-5" -&gt; {2, 5})
        /// </summary>
        /// <param name="minMaxString"></param>
        /// <returns></returns>
        public static int[] ParseMinMaxString(string minMaxString)
        {
            //Example input: 15-100
            string[] minMax = minMaxString.Split('-');
            return new int[] { int.Parse(minMax[0]), int.Parse(minMax[1]) };

        }
    }
}
