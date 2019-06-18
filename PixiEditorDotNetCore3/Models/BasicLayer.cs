using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using PixiEditor.Helpers;

namespace PixiEditorDotNetCore3.Models
{
    [Serializable]
    public class BasicLayer : NotifyableObject
    {

        private int _width;

        public int Width
        {
            get { return _width; }
            set { _width = value; RaisePropertyChanged("Width"); }
        }

        private int _height;

        public int Height
        {
            get { return _height; }
            set { _height = value; RaisePropertyChanged("Height"); }
        }
    }
}
