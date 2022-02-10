using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models.DataHolders
{
    public struct PixelSize
    {
        public int Height { get; set; }
        public int Width { get; set; }

        public PixelSize(int height, int width)
        {
            Height = height;
            Width = width;
        }

        public static PixelSize operator +(PixelSize first, PixelSize second) => new PixelSize(first.Width + second.Width, first.Height + second.Height);
    }
}
