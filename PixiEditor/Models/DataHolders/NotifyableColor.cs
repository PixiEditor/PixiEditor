using PixiEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PixiEditor.Models.DataHolders
{
    public class NotifyableColor : NotifyableObject
    {
        public event EventHandler ColorChanged;
        public byte A
        {
            get => Color.A;
            set
            {
                Color = System.Windows.Media.Color.FromArgb(value, Color.R, Color.G, Color.B);
            }
        }
        public byte R
        {
            get => _color.R;
            set
            {
                Color = System.Windows.Media.Color.FromArgb(Color.A, value, Color.G, Color.B);
            }
        }


        public byte G
        {
            get => Color.G;
            set
            {
                Color = System.Windows.Media.Color.FromArgb(Color.A, Color.R, value, Color.B);
            }
        }

        public byte B
        {
            get => Color.B;
            set
            {
                Color = System.Windows.Media.Color.FromArgb(Color.A, Color.R, Color.G, value);
            }
        }

        private System.Windows.Media.Color _color;

        public System.Windows.Media.Color Color
        {
            get => _color;
            set
            {
                _color = value;
                RaisePropertyChanged("Color");
                RaisePropertyChanged("A");
                RaisePropertyChanged("R");
                RaisePropertyChanged("G");
                RaisePropertyChanged("B");
                ColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public NotifyableColor(System.Windows.Media.Color color)
        {
            Color = color;
        }

    }
}
