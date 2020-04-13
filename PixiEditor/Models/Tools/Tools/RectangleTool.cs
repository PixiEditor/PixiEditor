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

        public RectangleTool()
        {
            Tooltip = "Draws rectanlge on cavnas (R)";
        }

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

            for (int i = coordinates.Coords1.X; i < coordinates.Coords2.X + 1; i++)
            {
                finalCoordinates.Add(new Coordinates(i, coordinates.Coords1.Y));
                finalCoordinates.Add(new Coordinates(i, coordinates.Coords2.Y));
            }
            for (int i = coordinates.Coords1.Y + 1; i <= coordinates.Coords2.Y - 1; i++)
            {
                finalCoordinates.Add(new Coordinates(coordinates.Coords1.X, i));
                finalCoordinates.Add(new Coordinates(coordinates.Coords2.X, i));
            }

            if (filled)
            {
                finalCoordinates.AddRange(CalculatedFillForRectangle(coordinates));
            }
            finalCoordinates = finalCoordinates.Distinct().ToList();
            return finalCoordinates.ToArray();
        }

        private Coordinates[] CalculatedFillForRectangle(DoubleCords cords)
        {
            int height = cords.Coords2.Y - cords.Coords1.Y;
            int width = cords.Coords2.X - cords.Coords1.X;
            Coordinates[] filledCoordinates = new Coordinates[width * height];
            int i = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    filledCoordinates[i] = new Coordinates(cords.Coords1.X + x, cords.Coords1.Y + y);
                    i++;
                }
            }
            return filledCoordinates;
        }
    }
}
