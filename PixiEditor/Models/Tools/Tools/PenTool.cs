using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace PixiEditor.Models.Tools.Tools
{
    public class PenTool : Tool
    {
        public override ToolType ToolType => ToolType.Pen;

        public PenTool()
        {
            Cursor = Cursors.Pen;
            Tooltip = "Standard brush (B)";
        }

        public override BitmapPixelChanges Use(Layer layer, Coordinates[] coordinates, Color color, int toolSize)
        {
            return Draw(coordinates[0], color, toolSize);
        }

        public BitmapPixelChanges Draw(Coordinates startingCoords, Color color, int toolSize)
        {
            int x1, y1, x2, y2;
            DoubleCords centeredCoords = CoordinatesCalculator.CalculateThicknessCenter(startingCoords, toolSize);
            x1 = centeredCoords.Coords1.X;
            y1 = centeredCoords.Coords1.Y;
            x2 = centeredCoords.Coords2.X;
            y2 = centeredCoords.Coords2.Y;
            return BitmapPixelChanges.FromSingleColoredArray(CoordinatesCalculator.RectangleToCoordinates(x1, y1, x2, y2), color);
        }
    }
}
