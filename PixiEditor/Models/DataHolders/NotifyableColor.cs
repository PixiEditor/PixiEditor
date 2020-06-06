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

        private byte _a;
        public byte A
        {
            get => _a;
            set
            {
                _a = value;
                ColorChanged?.Invoke(this, EventArgs.Empty);
                RaisePropertyChanged("A");

            }
        }

        private byte _r;
        public byte R
        {
            get => _r;
            set
            {
                _r = value;
                ColorChanged?.Invoke(this, EventArgs.Empty);
                RaisePropertyChanged("R");

            }
        }


        private byte _g;
        public byte G
        {
            get => _g;
            set
            {
                _g = value;
                ColorChanged?.Invoke(this, EventArgs.Empty);
                RaisePropertyChanged("G");

            }
        }

        private byte _b;
        public byte B
        {
            get => _b;
            set
            {
                _b = value;
                ColorChanged?.Invoke(this, EventArgs.Empty);
                RaisePropertyChanged("B");
            }
        }

        public void SetArgb(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public NotifyableColor(System.Windows.Media.Color color)
        {
            A = color.A;
            R = color.R;
            G = color.G;
            B = color.B;
        }

    }
}
