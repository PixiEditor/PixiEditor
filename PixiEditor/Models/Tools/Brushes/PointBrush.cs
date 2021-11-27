using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models.Tools.Brushes
{
    public class PointBrush : CustomBrush
    {
        public static readonly int[,] PointMatrix = new int[,]
        {
            { 1 }
        };

        public override int[,] BrushMatrix => PointMatrix;
    }
}
