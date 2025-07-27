using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorManagerCS_GUI.Core
{
    public class IndexedPropertyChangedArgs : PropertyChangedEventArgs
    {
        public int Index { get; }

        public IndexedPropertyChangedArgs(string propertyName, int index)
            : base(propertyName)
        {
            Index = index;
        }
    }
}
