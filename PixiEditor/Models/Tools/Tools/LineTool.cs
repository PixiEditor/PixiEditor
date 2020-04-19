using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace PixiEditor.Models.Tools.Tools
{
	public class LineTool : ShapeTool
    {

        public override ToolType ToolType => ToolType.Line;

        public LineTool()
        {
            Tooltip = "Draws line on canvas (L)";
        }

        public override BitmapPixelChanges Use(Layer layer, Coordinates[] coordinates, Color color, int toolSize)
        {
            return BitmapPixelChanges.FromSingleColoredArray(CreateLine(coordinates, toolSize), color);
        }

        public Coordinates[] CreateLine(Coordinates[] coordinates, int thickness)
        {
            Coordinates startingCoordinates = coordinates[^1];
            Coordinates latestCoordinates = coordinates[0];
            return BresenhamLine(startingCoordinates.X, startingCoordinates.Y, latestCoordinates.X, latestCoordinates.Y);
        }
        

        public Coordinates[] ThickLine(Coordinates start, Coordinates end, int thickness)
        {
            List<Coordinates> points = new List<Coordinates>();

            Coordinates[] linePoints = BresenhamLine(start.X, start.Y, end.X, end.Y);

            

            return points.Distinct().ToArray();
		}       
	}
}
