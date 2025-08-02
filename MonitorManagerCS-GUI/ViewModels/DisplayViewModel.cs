using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MonitorManagerCS_GUI.Core;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
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

                    UpdateActualBounds();
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

        private Rect _actualBounds;
        public Rect ActualBounds { get => _actualBounds; }

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

        public Brush BorderBrush
        {
            get
            {
                if (_isHighlighted)
                {
                    return BorderBrushHighlight;
                }
                
                return BorderBrushNormal;
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

        private Thickness _padding = new Thickness(2);
        public Thickness Padding
        {
            get => _padding;
            set
            {
                if (!Equals(_padding, value))
                {
                    _padding = value;
                    UpdateActualBounds();
                }
            }
        }

        private Brush _borderBrushNormal = Brushes.Transparent;
        public Brush BorderBrushNormal
        {
            get => _borderBrushNormal;
            set
            {
                if (!Equals(_borderBrushNormal, value))
                {
                    _borderBrushNormal = value;
                    OnPropertyChanged(nameof(BorderBrush));
                    OnPropertyChanged();
                }
            }
        }

        private Brush _borderBrushHighlight = Brushes.Black;
        public Brush BorderBrushHighlight
        {
            get => _borderBrushHighlight;
            set
            {
                if (Equals(_borderBrushHighlight, value))
                {
                    _borderBrushHighlight = value;
                    OnPropertyChanged(nameof(BorderBrush));
                    OnPropertyChanged();
                }
            }
        }
        private bool _isHighlighted = false;

        private void UpdateActualBounds()
        {
            if (Bounds == Rect.Empty) return;

            _actualBounds = new Rect(
                _bounds.Left * _scale + _padding.Left, 
                _bounds.Top * _scale + _padding.Top,
                _bounds.Width * _scale - _padding.Left - _padding.Right, 
                _bounds.Height * _scale - _padding.Top - _padding.Bottom);

            OnPropertyChanged(nameof(ActualBounds));
        }
        private void SetBounds(Rect newBounds)
        {
            _bounds = newBounds;
            OnPropertyChanged(nameof(Bounds));

            UpdateActualBounds();

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

        private RelayCommand _mouseEnterCommand;
        public ICommand MouseEnterCommand
        {
            get
            {
                if (_mouseEnterCommand == null)
                {
                    _mouseEnterCommand = new RelayCommand(OnMouseEnter);
                }

                return _mouseEnterCommand;
            }
        }
        private void OnMouseEnter()
        {
            _isHighlighted = true;
            OnPropertyChanged(nameof(BorderBrush));
        }

        private RelayCommand _mouseLeaveCommand;
        public ICommand MouseLeaveCommand
        {
            get
            {
                if (_mouseLeaveCommand == null)
                {
                    _mouseLeaveCommand = new RelayCommand(OnMouseLeave);
                }

                return _mouseLeaveCommand;
            }
        }
        private void OnMouseLeave()
        {
            _isHighlighted = false;
            OnPropertyChanged(nameof(BorderBrush));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
