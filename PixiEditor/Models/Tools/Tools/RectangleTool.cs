using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;

namespace PixiEditor.Models.Tools.Tools
{
    public class RectangleTool : ShapeTool
    {
        public override ToolType ToolType => ToolType.Rectangle;
        public bool Filled { get; set; } = false;

        public RectangleTool()
        {
            Tooltip = "Draws rectangle on canvas (R). Hold Shift to draw square.";
        }

        public override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            int thickness = (int) Toolbar.GetSetting("ToolSize").Value;
            BitmapPixelChanges pixels =
                BitmapPixelChanges.FromSingleColoredArray(CreateRectangle(coordinates, thickness), color);
            if ((bool) Toolbar.GetSetting("Fill").Value)
            {
                Color fillColor = (Color) Toolbar.GetSetting("FillColor").Value;
                pixels.ChangedPixels.AddRangeOverride(
                    BitmapPixelChanges.FromSingleColoredArray
                            (CalculateFillForRectangle(coordinates[^1], coordinates[0], thickness), fillColor)
                        .ChangedPixels);
            }

            return new[] {new LayerChange(pixels, layer)};
        }

        public IEnumerable<Coordinates> CreateRectangle(Coordinates[] coordinates, int thickness)
        {
            DoubleCords fixedCoordinates = CalculateCoordinatesForShapeRotation(coordinates[^1], coordinates[0]);
            List<Coordinates> output = new List<Coordinates>();
            IEnumerable<Coordinates> rectangle = CalculateRectanglePoints(fixedCoordinates);
            output.AddRange(rectangle);

            for (int i = 1; i < (int) Math.Floor(thickness / 2f) + 1; i++)
                output.AddRange(CalculateRectanglePoints(new DoubleCords(
                    new Coordinates(fixedCoordinates.Coords1.X - i, fixedCoordinates.Coords1.Y - i),
                    new Coordinates(fixedCoordinates.Coords2.X + i, fixedCoordinates.Coords2.Y + i))));
            for (int i = 1; i < (int) Math.Ceiling(thickness / 2f); i++)
                output.AddRange(CalculateRectanglePoints(new DoubleCords(
                    new Coordinates(fixedCoordinates.Coords1.X + i, fixedCoordinates.Coords1.Y + i),
                    new Coordinates(fixedCoordinates.Coords2.X - i, fixedCoordinates.Coords2.Y - i))));

            return output.Distinct();
        }

        public IEnumerable<Coordinates> CreateRectangle(Coordinates start, Coordinates end, int thickness)
        {
            return CreateRectangle(new[] {end, start}, thickness);
        }

        private IEnumerable<Coordinates> CalculateRectanglePoints(DoubleCords coordinates)
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

            return finalCoordinates;
        }

        public IEnumerable<Coordinates> CalculateFillForRectangle(Coordinates start, Coordinates end, int thickness)
        {
            int offset = (int) Math.Ceiling(thickness / 2f);
            DoubleCords fixedCords = CalculateCoordinatesForShapeRotation(start, end);

            DoubleCords innerCords = new DoubleCords
            {
                Coords1 = new Coordinates(fixedCords.Coords1.X + offset, fixedCords.Coords1.Y + offset),
                Coords2 = new Coordinates(fixedCords.Coords2.X - (offset - 1), fixedCords.Coords2.Y - (offset - 1))
            };

            int height = innerCords.Coords2.Y - innerCords.Coords1.Y;
            int width = innerCords.Coords2.X - innerCords.Coords1.X;

            if (height < 1 || width < 1) return Array.Empty<Coordinates>();
            Coordinates[] filledCoordinates = new Coordinates[width * height];
            int i = 0;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                filledCoordinates[i] = new Coordinates(innerCords.Coords1.X + x, innerCords.Coords1.Y + y);
                i++;
            }

            return filledCoordinates.Distinct();
        }
    }
}