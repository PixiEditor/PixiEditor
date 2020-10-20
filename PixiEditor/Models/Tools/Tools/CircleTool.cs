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
    public class CircleTool : ShapeTool
    {
        public CircleTool()
        {
            Tooltip = "Draws circle on canvas (C). Hold Shift to draw even circle.";
        }

        public override ToolType ToolType => ToolType.Circle;

        public override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            var thickness = Toolbar.GetSetting<SizeSetting>("ToolSize").Value;
            var fixedCoordinates = CalculateCoordinatesForShapeRotation(coordinates[^1], coordinates[0]);
            var outline = CreateEllipse(fixedCoordinates.Coords1, fixedCoordinates.Coords2, thickness);
            var pixels = BitmapPixelChanges.FromSingleColoredArray(outline, color);
            if (Toolbar.GetSetting<BoolSetting>("Fill").Value)
            {
                var fillColor = Toolbar.GetSetting<ColorSetting>("FillColor").Value;
                pixels.ChangedPixels.AddRangeNewOnly(
                    BitmapPixelChanges.FromSingleColoredArray(CalculateFillForEllipse(outline), fillColor)
                        .ChangedPixels);
            }

            return new[] {new LayerChange(pixels, layer)};
        }

        /// <summary>
        ///     Calculates ellipse points for specified coordinates and thickness.
        /// </summary>
        /// <param name="startCoordinates">Top left coordinate of ellipse</param>
        /// <param name="endCoordinates">Bottom right coordinate of ellipse</param>
        /// <param name="thickness">Thickness of ellipse</param>
        /// <param name="filled">Should ellipse be filled</param>
        /// <returns>Coordinates for ellipse</returns>
        public IEnumerable<Coordinates> CreateEllipse(Coordinates startCoordinates, Coordinates endCoordinates, int thickness,
            bool filled)
        {
            var output = new List<Coordinates>();
            var outline = CreateEllipse(startCoordinates, endCoordinates, thickness);
            output.AddRange(outline);
            if (filled)
            {
                output.AddRange(CalculateFillForEllipse(outline));
                return output.Distinct();
            }

            return output;
        }

        /// <summary>
        ///     Calculates ellipse points for specified coordinates and thickness.
        /// </summary>
        /// <param name="startCoordinates">Top left coordinate of ellipse</param>
        /// <param name="endCoordinates">Bottom right coordinate of ellipse</param>
        /// <param name="thickness">Thickness of ellipse</param>
        /// <returns>Coordinates for ellipse</returns>
        public IEnumerable<Coordinates> CreateEllipse(Coordinates startCoordinates, Coordinates endCoordinates, int thickness)
        {
            var radiusX = (endCoordinates.X - startCoordinates.X) / 2.0;
            var radiusY = (endCoordinates.Y - startCoordinates.Y) / 2.0;
            var centerX = (startCoordinates.X + endCoordinates.X + 1) / 2.0;
            var centerY = (startCoordinates.Y + endCoordinates.Y + 1) / 2.0;

            var output = new List<Coordinates>();
            var ellipse = MidpointEllipse(radiusX, radiusY, centerX, centerY);
            if (thickness == 1)
                output.AddRange(ellipse);
            else
                output.AddRange(GetThickShape(ellipse, thickness));
            return output.Distinct();
        }

        public IEnumerable<Coordinates> MidpointEllipse(double halfWidth, double halfHeight, double centerX, double centerY)
        {
            if (halfWidth < 1 || halfHeight < 1)
                return FallbackRectangle(halfWidth, halfHeight, centerX, centerY);

            //ellipse formula: halfHeight^2 * x^2 + halfWidth^2 * y^2 - halfHeight^2 * halfWidth^2 = 0

            //Make sure we are always at the center of a pixel
            var currentX = Math.Ceiling(centerX - 0.5) + 0.5;
            var currentY = centerY + halfHeight;

            var outputCoordinates = new List<Coordinates>();

            double currentSlope;

            //from PI/2 to middle
            do
            {
                outputCoordinates.AddRange(GetRegionPoints(currentX, centerX, currentY, centerY));

                //calculate next pixel coords
                currentX++;

                if (Math.Pow(halfHeight, 2) * Math.Pow(currentX - centerX, 2) +
                    Math.Pow(halfWidth, 2) * Math.Pow(currentY - centerY - 0.5, 2) -
                    Math.Pow(halfWidth, 2) * Math.Pow(halfHeight, 2) >= 0)
                    currentY--;

                //calculate how far we've advanced
                var derivativeX = 2 * Math.Pow(halfHeight, 2) * (currentX - centerX);
                var derivativeY = 2 * Math.Pow(halfWidth, 2) * (currentY - centerY);
                currentSlope = -(derivativeX / derivativeY);
            } while (currentSlope > -1 && currentY - centerY > 0.5);

            //from middle to 0
            while (currentY - centerY >= 0)
            {
                outputCoordinates.AddRange(GetRegionPoints(currentX, centerX, currentY, centerY));

                currentY--;
                if (Math.Pow(halfHeight, 2) * Math.Pow(currentX - centerX + 0.5, 2) +
                    Math.Pow(halfWidth, 2) * Math.Pow(currentY - centerY, 2) -
                    Math.Pow(halfWidth, 2) * Math.Pow(halfHeight, 2) < 0)
                    currentX++;
            }

            return outputCoordinates;
        }

        private Coordinates[] FallbackRectangle(double halfWidth, double halfHeight, double centerX, double centerY)
        {
            var coordinates = new List<Coordinates>();
            for (var x = centerX - halfWidth; x <= centerX + halfWidth; x++)
            {
                coordinates.Add(new Coordinates((int) x, (int) (centerY - halfHeight)));
                coordinates.Add(new Coordinates((int) x, (int) (centerY + halfHeight)));
            }

            for (var y = centerY - halfHeight + 1; y <= centerY + halfHeight - 1; y++)
            {
                coordinates.Add(new Coordinates((int) (centerX - halfWidth), (int) y));
                coordinates.Add(new Coordinates((int) (centerX + halfWidth), (int) y));
            }

            return coordinates.ToArray();
        }

        private IEnumerable<Coordinates> CalculateFillForEllipse(IEnumerable<Coordinates> outlineCoordinates)
        {
            var finalCoordinates = new List<Coordinates>();
            var bottom = outlineCoordinates.Max(x => x.Y);
            var top = outlineCoordinates.Min(x => x.Y);
            for (var i = top + 1; i < bottom; i++)
            {
                var rowCords = outlineCoordinates.Where(x => x.Y == i);
                var right = rowCords.Max(x => x.X);
                var left = rowCords.Min(x => x.X);
                for (var j = left + 1; j < right; j++) finalCoordinates.Add(new Coordinates(j, i));
            }

            return finalCoordinates;
        }

        private Coordinates[] GetRegionPoints(double x, double xc, double y, double yc)
        {
            var outputCoordinates = new Coordinates[4];
            outputCoordinates[0] = new Coordinates((int) Math.Floor(x), (int) Math.Floor(y));
            outputCoordinates[1] = new Coordinates((int) Math.Floor(-(x - xc) + xc), (int) Math.Floor(y));
            outputCoordinates[2] = new Coordinates((int) Math.Floor(x), (int) Math.Floor(-(y - yc) + yc));
            outputCoordinates[3] = new Coordinates((int) Math.Floor(-(x - xc) + xc), (int) Math.Floor(-(y - yc) + yc));
            return outputCoordinates;
        }
    }
}