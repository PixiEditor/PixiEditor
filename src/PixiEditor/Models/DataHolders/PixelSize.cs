using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models.DataHolders
{
    public struct PixelSize
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public PixelSize(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
