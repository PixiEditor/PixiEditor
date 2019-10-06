using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace PixiEditorDotNetCore3.Models.Tools.Tools
{
    public class PenTool : Tool
    {
        public override ToolType ToolType => ToolType.Pen;


        public override BitmapPixelChanges Use(Layer layer, Coordinates startingCoords, Color color, int toolSize)
        {
            return Draw(startingCoords, color, toolSize);
        }

        public BitmapPixelChanges Draw(Coordinates startingCoords, Color color, int toolSize)
        {
            int x1, y1, x2, y2;
            DoubleCords centeredCoords = CoordinatesCalculator.CalculateThicknessCenter(startingCoords, toolSize);
            x1 = centeredCoords.Coords1.X;
            y1 = centeredCoords.Coords1.Y;
            x2 = centeredCoords.Coords2.X;
            y2 = centeredCoords.Coords2.Y;
            return new BitmapPixelChanges(CoordinatesCalculator.RectangleToCoordinates(x1, y1, x2, y2), color);
        }
    }
}
