using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models.Tools.Brushes
{
    public class CrossBrush : CustomBrush
    {
        public static readonly int[,] CrossMatrix = new int[3, 3]
        {
            { 0, 1, 0 },
            { 1, 1, 1 },
            { 0, 1, 0 }
        };

        public override int[,] BrushMatrix => CrossMatrix;
    }
}
