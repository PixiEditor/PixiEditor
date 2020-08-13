using System;
using ReactiveUI;

namespace PixiEditor.Models.Layers
{
    [Serializable]
    public class BasicLayer : ReactiveObject
    {
        public int Width
        {
            get => _width;
            set
            {
                _width = value;
                this.RaisePropertyChanged("Width");
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                this.RaisePropertyChanged("Height");
            }
        }

        private int _height;

        private int _width;
    }
}