using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models.Tools
{
    /// <summary>
    /// Represents a linear range to be filled and branched from.
    /// </summary>
    public struct FloodFillRange
    {
        public int StartX;
        public int EndX;
        public int Y;

        public FloodFillRange(int startX, int endX, int y)
        {
            StartX = startX;
            EndX = endX;
            Y = y;
        }
    }
}
