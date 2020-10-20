using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;

namespace PixiEditor.Models.Tools.Tools
{
    public class RectangleTool : ShapeTool
    {
        public RectangleTool()
        {
            Tooltip = "Draws rectangle on canvas (R). Hold Shift to draw square.";
        }

        public override ToolType ToolType => ToolType.Rectangle;
        public bool Filled { get; set; } = false;

        public override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            var thickness = Toolbar.GetSetting<SizeSetting>("ToolSize").Value;
            var pixels =
                BitmapPixelChanges.FromSingleColoredArray(CreateRectangle(coordinates, thickness), color);
            if (Toolbar.GetSetting<BoolSetting>("Fill").Value)
            {
                var fillColor = Toolbar.GetSetting<ColorSetting>("FillColor").Value;
                pixels.ChangedPixels.AddRangeOverride(
                    BitmapPixelChanges.FromSingleColoredArray
                            (CalculateFillForRectangle(coordinates[^1], coordinates[0], thickness), fillColor)
                        .ChangedPixels);
            }

            return new[] {new LayerChange(pixels, layer)};
        }

        public IEnumerable<Coordinates> CreateRectangle(Coordinates[] coordinates, int thickness)
        {
            var fixedCoordinates = CalculateCoordinatesForShapeRotation(coordinates[^1], coordinates[0]);
            var output = new List<Coordinates>();
            var rectangle = CalculateRectanglePoints(fixedCoordinates);
            output.AddRange(rectangle);

            for (var i = 1; i < (int) Math.Floor(thickness / 2f) + 1; i++)
                output.AddRange(CalculateRectanglePoints(new DoubleCords(
                    new Coordinates(fixedCoordinates.Coords1.X - i, fixedCoordinates.Coords1.Y - i),
                    new Coordinates(fixedCoordinates.Coords2.X + i, fixedCoordinates.Coords2.Y + i))));
            for (var i = 1; i < (int) Math.Ceiling(thickness / 2f); i++)
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
            var finalCoordinates = new List<Coordinates>();

            for (var i = coordinates.Coords1.X; i < coordinates.Coords2.X + 1; i++)
            {
                finalCoordinates.Add(new Coordinates(i, coordinates.Coords1.Y));
                finalCoordinates.Add(new Coordinates(i, coordinates.Coords2.Y));
            }

            for (var i = coordinates.Coords1.Y + 1; i <= coordinates.Coords2.Y - 1; i++)
            {
                finalCoordinates.Add(new Coordinates(coordinates.Coords1.X, i));
                finalCoordinates.Add(new Coordinates(coordinates.Coords2.X, i));
            }

            return finalCoordinates;
        }

        public IEnumerable<Coordinates> CalculateFillForRectangle(Coordinates start, Coordinates end, int thickness)
        {
            var offset = (int) Math.Ceiling(thickness / 2f);
            var fixedCords = CalculateCoordinatesForShapeRotation(start, end);

            var innerCords = new DoubleCords
            {
                Coords1 = new Coordinates(fixedCords.Coords1.X + offset, fixedCords.Coords1.Y + offset),
                Coords2 = new Coordinates(fixedCords.Coords2.X - (offset - 1), fixedCords.Coords2.Y - (offset - 1))
            };

            var height = innerCords.Coords2.Y - innerCords.Coords1.Y;
            var width = innerCords.Coords2.X - innerCords.Coords1.X;

            if (height < 1 || width < 1) return Array.Empty<Coordinates>();
            var filledCoordinates = new Coordinates[width * height];
            var i = 0;
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                filledCoordinates[i] = new Coordinates(innerCords.Coords1.X + x, innerCords.Coords1.Y + y);
                i++;
            }

            return filledCoordinates.Distinct();
        }
    }
}