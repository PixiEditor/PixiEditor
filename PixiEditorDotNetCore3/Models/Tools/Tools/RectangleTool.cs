using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace PixiEditor.Models.Tools.Tools
{
    public class RectangleTool : ShapeTool
    {
        public override ToolType ToolType => ToolType.Rectangle;
        public bool Filled { get; set; } = false;

        public override BitmapPixelChanges Use(Layer layer, Coordinates[] coordinates, Color color, int toolSize)
        {
            return BitmapPixelChanges.FromSingleColoredArray(CreateRectangle(coordinates, toolSize), color);
        }

        public Coordinates[] CreateRectangle(Coordinates[] coordinates, int toolSize)
        {
            DoubleCords fixedCoordinates = CalculateCoordinatesForShapeRotation(coordinates[^1], coordinates[0]);
            return CalculateRectanglePoints(fixedCoordinates, Filled);
        }

        private Coordinates[] CalculateRectanglePoints(DoubleCords coordinates, bool filled)
        {
            List<Coordinates> finalCoordinates = new List<Coordinates>();
            for (int i = coordinates.Coords1.X; i < coordinates.Coords2.X; i++)
            {
                finalCoordinates.Add(new Coordinates(i, coordinates.Coords1.Y));
                finalCoordinates.Add(new Coordinates(i, coordinates.Coords2.Y));
            }
            for (int i = coordinates.Coords1.Y; i < coordinates.Coords2.Y + 1; i++)
            {
                finalCoordinates.Add(new Coordinates(coordinates.Coords1.X, i));
                finalCoordinates.Add(new Coordinates(coordinates.Coords2.X, i));
            }
            finalCoordinates = finalCoordinates.Distinct().ToList();
            return finalCoordinates.ToArray();
        }
    }
}
