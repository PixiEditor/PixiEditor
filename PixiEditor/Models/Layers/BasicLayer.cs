using System;
using PixiEditor.Helpers;

namespace PixiEditor.Models.Layers
{
    [Serializable]
    public class BasicLayer : NotifyableObject
    {
        private int height;

        private int width;

        public int Width
        {
            get => width;
            set
            {
                width = value;
                RaisePropertyChanged("Width");
            }
        }

        public int Height
        {
            get => height;
            set
            {
                height = value;
                RaisePropertyChanged("Height");
            }
        }
    }
}