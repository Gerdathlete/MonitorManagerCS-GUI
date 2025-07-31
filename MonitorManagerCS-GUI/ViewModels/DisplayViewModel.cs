using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MonitorManagerCS_GUI.ViewModels
{
    public class DisplayViewModel : INotifyPropertyChanged
    {
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
                    OnPropertyChanged();

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
            }
        }


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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
