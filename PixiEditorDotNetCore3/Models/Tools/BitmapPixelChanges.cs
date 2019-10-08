using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace PixiEditorDotNetCore3.Models.Tools
{
     public struct BitmapPixelChanges
    {
        public Coordinates[] ChangedCoordinates { get; set; }
        public Color PixelsColor { get; set; }

        public BitmapPixelChanges(Coordinates[] changedCoordinates, Color color)
        {
            ChangedCoordinates = changedCoordinates;
            PixelsColor = color;
        }
    }
}
