using PixiEditor.Helpers;
using System;

namespace PixiEditor.Models.Layers
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
