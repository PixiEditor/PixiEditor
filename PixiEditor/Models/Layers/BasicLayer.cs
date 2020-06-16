using System;
using PixiEditor.Helpers;

namespace PixiEditor.Models.Layers
{
    [Serializable]
    public class BasicLayer : NotifyableObject
    {
        private int _height;

        private int _width;

        public int Width
        {
            get => _width;
            set
            {
                _width = value;
                RaisePropertyChanged("Width");
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }
    }
}