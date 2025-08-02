using CommunityToolkit.Mvvm.ComponentModel;
using MonitorManagerCS_GUI.Core;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace MonitorManagerCS_GUI.ViewModels
{
    public class DisplayViewModel : INotifyPropertyChanged
    {
        private double _scale = 1;
        public double Scale
        {
            get { return _scale; }
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    OnPropertyChanged();

                    UpdateScaledBounds();
                }
            }
        }

        private double _x;
        public double X
        {
            get { return _x; }
            set
            {
                if (_x != value)
                {
                    _x = value;
                    OnPropertyChanged();

                    Bounds = new Rect(_x, _y, _width, _height);
                }
            }
        }

        private double _y;
        public double Y
        {
            get { return _y; }
            set
            {
                if (_y != value)
                {
                    _y = value;
                    OnPropertyChanged();

                    Bounds = new Rect(_x, _y, _width, _height);
                }
            }
        }

        private double _width;
        public double Width
        {
            get { return _width; }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged();

                    Bounds = new Rect(_x, _y, _width, _height);
                }
            }
        }

        private double _height;
        public double Height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged();

                    Bounds = new Rect(_x, _y, _width, _height);
                }
            }
        }

        private Point _position;
        public Point Position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged();

                    Bounds = new Rect(_x, _y, _width, _height);
                }
            }
        }

        private Size _size;
        public Size Size
        {
            get { return _size; }
            set
            {
                if (_size != value)
                {
                    _size = value;
                    OnPropertyChanged();

                    Bounds = new Rect(_x, _y, _width, _height);
                }
            }
        }

        private Rect _bounds;
        public Rect Bounds
        {
            get { return _bounds; }
            set
            {
                if (_bounds != value)
                {
                    _bounds = value;
                    SetBounds(_bounds);
                }
            }
        }

        private Rect _scaledBounds;
        public Rect ScaledBounds { get => _scaledBounds; }

        private string _label;
        public string Label
        {
            get { return _label; }
            set
            {
                if (_label != value)
                {
                    _label = value;
                    OnPropertyChanged();
                }
            }
        }

        private Brush _background = new SolidColorBrush(Color.FromArgb(255, 0xDA, 0xDA, 0xDA));
        public Brush Background
        {
            get { return _background; }
            set
            {
                if (_background != value)
                {
                    _background = value; 
                    OnPropertyChanged();
                }
            }
        }

        private Brush _borderBrush = Brushes.Black;
        public Brush BorderBrush
        {
            get => _borderBrush;
            set
            {
                if (!Equals(_borderBrush, value))
                {
                    _borderBrush = value;
                    OnPropertyChanged(nameof(BorderBrush));
                }
            }
        }

        private Thickness _borderThickness = new Thickness(1);
        public Thickness BorderThickness
        {
            get => _borderThickness;
            set
            {
                if (!Equals(_borderThickness, value))
                {
                    _borderThickness = value;
                    OnPropertyChanged(nameof(BorderThickness));
                }
            }
        }

        private void UpdateScaledBounds()
        {
            if (Bounds == Rect.Empty) return;

            _scaledBounds = new Rect(_bounds.Left * _scale, _bounds.Top * _scale,
                _bounds.Width * _scale, _bounds.Height * _scale);

            OnPropertyChanged(nameof(ScaledBounds));
        }
        private void SetBounds(Rect newBounds)
        {
            _bounds = newBounds;
            OnPropertyChanged(nameof(Bounds));

            UpdateScaledBounds();

            _x = _bounds.Left;
            _y = _bounds.Top;
            _width = _bounds.Width;
            _height = _bounds.Height;
            _position = _bounds.Location;
            _size = _bounds.Size;

            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(Position));
            OnPropertyChanged(nameof(Size));
        }

        public DisplayViewModel(DisplayInfo displayInfo, double scale)
        {
            _scale = scale;

            string serial = displayInfo.SerialNumber == string.Empty
                ? "(Not Set)"
                : displayInfo.SerialNumber;

            _label = displayInfo.ToString() + Environment.NewLine
                + $"SN: {serial}";

            System.Drawing.Rectangle rect = displayInfo.Bounds;
            SetBounds(new Rect(rect.X, rect.Y, rect.Width, rect.Height));
        }
        public DisplayViewModel(double x, double y, double width, double height, double scale)
        {
            _scale = scale;
            SetBounds(new Rect(x, y, width, height));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
