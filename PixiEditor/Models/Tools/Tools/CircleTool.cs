using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace PixiEditor.Models.Tools.Tools
{
    public class CircleTool : ShapeTool
    {
        public CircleTool()
        {
            ActionDisplay = "Click and move mouse to draw a circle. Hold Shift to draw an even one.";
        }

        public override string Tooltip => "Draws circle on canvas (C). Hold Shift to draw even circle.";

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                ActionDisplay = "Click and move mouse to draw an even circle.";
            }
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                ActionDisplay = "Click and move mouse to draw a circle. Hold Shift to draw an even one.";
            }
        }

        public override void Use(Layer layer, List<Coordinates> coordinates, SKColor color)
        {
            int thickness = Toolbar.GetSetting<SizeSetting>("ToolSize").Value;
            DoubleCords fixedCoordinates = CalculateCoordinatesForShapeRotation(coordinates[^1], coordinates[0]);
            var outline = CreateEllipse(layer, color, fixedCoordinates.Coords1, fixedCoordinates.Coords2, thickness);

            if (Toolbar.GetSetting<BoolSetting>("Fill").Value)
            {
                Color temp = Toolbar.GetSetting<ColorSetting>("FillColor").Value;
                SKColor fillColor = new SKColor(temp.R, temp.G, temp.B, temp.A);
                DrawEllipseFill(layer, fillColor, outline);
            }
        }

        /// <summary>
        ///     Draws ellipse for specified coordinates and thickness.
        /// </summary>
        /// <param name="startCoordinates">Top left coordinate of ellipse.</param>
        /// <param name="endCoordinates">Bottom right coordinate of ellipse.</param>
        /// <param name="thickness">Thickness of ellipse.</param>
        /// <param name="filled">Should ellipse be filled.</param>
        public void CreateEllipse(Layer layer, SKColor color, Coordinates startCoordinates, Coordinates endCoordinates, int thickness, bool filled)
        {
            IEnumerable<Coordinates> outline = CreateEllipse(layer, color, startCoordinates, endCoordinates, thickness);
            if (filled)
            {
                DrawEllipseFill(layer, color, outline);
            }
        }

        /// <summary>
        ///     Calculates ellipse points for specified coordinates and thickness.
        /// </summary>
        /// <param name="startCoordinates">Top left coordinate of ellipse.</param>
        /// <param name="endCoordinates">Bottom right coordinate of ellipse.</param>
        /// <param name="thickness">Thickness of ellipse.</param>
        public IEnumerable<Coordinates> CreateEllipse(Layer layer, SKColor color, Coordinates startCoordinates, Coordinates endCoordinates, int thickness)
        {
            double radiusX = (endCoordinates.X - startCoordinates.X) / 2.0;
            double radiusY = (endCoordinates.Y - startCoordinates.Y) / 2.0;
            double centerX = (startCoordinates.X + endCoordinates.X + 1) / 2.0;
            double centerY = (startCoordinates.Y + endCoordinates.Y + 1) / 2.0;

            IEnumerable<Coordinates> ellipse = GenerateMidpointEllipse(layer, color, radiusX, radiusY, centerX, centerY);
            if (thickness > 1)
            {
                ThickenShape(layer, color, ellipse, thickness);
            }

            return ellipse;
        }

        public List<Coordinates> GenerateMidpointEllipse(Layer layer, SKColor color, double halfWidth, double halfHeight, double centerX, double centerY)
        {
            //using var ctx = layer.LayerBitmap.GetBitmapContext();

            if (halfWidth < 1 || halfHeight < 1)
            {
                return DrawFallbackRectangle(layer, color, halfWidth, halfHeight, centerX, centerY);
            }

            // ellipse formula: halfHeight^2 * x^2 + halfWidth^2 * y^2 - halfHeight^2 * halfWidth^2 = 0

            // Make sure we are always at the center of a pixel
            double currentX = Math.Ceiling(centerX - 0.5) + 0.5;
            double currentY = centerY + halfHeight;

            List<Coordinates> outputCoordinates = new List<Coordinates>();

            double currentSlope;

            // from PI/2 to middle
            do
            {
                outputCoordinates.AddRange(DrawRegionPoints(layer, color, currentX, centerX, currentY, centerY));

                // calculate next pixel coords
                currentX++;

                if ((Math.Pow(halfHeight, 2) * Math.Pow(currentX - centerX, 2)) +
                    (Math.Pow(halfWidth, 2) * Math.Pow(currentY - centerY - 0.5, 2)) -
                    (Math.Pow(halfWidth, 2) * Math.Pow(halfHeight, 2)) >= 0)
                {
                    currentY--;
                }

                // calculate how far we've advanced
                double derivativeX = 2 * Math.Pow(halfHeight, 2) * (currentX - centerX);
                double derivativeY = 2 * Math.Pow(halfWidth, 2) * (currentY - centerY);
                currentSlope = -(derivativeX / derivativeY);
            }
            while (currentSlope > -1 && currentY - centerY > 0.5);

            // from middle to 0
            while (currentY - centerY >= 0)
            {
                outputCoordinates.AddRange(DrawRegionPoints(layer, color, currentX, centerX, currentY, centerY));

                currentY--;
                if ((Math.Pow(halfHeight, 2) * Math.Pow(currentX - centerX + 0.5, 2)) +
                    (Math.Pow(halfWidth, 2) * Math.Pow(currentY - centerY, 2)) -
                    (Math.Pow(halfWidth, 2) * Math.Pow(halfHeight, 2)) < 0)
                {
                    currentX++;
                }
            }

            return outputCoordinates;
        }

        public void DrawEllipseFill(Layer layer, SKColor color, IEnumerable<Coordinates> outlineCoordinates)
        {
            //using var ctx = layer.LayerBitmap.GetBitmapContext();

            if (!outlineCoordinates.Any())
            {
                return;
            }

            int bottom = outlineCoordinates.Max(x => x.Y);
            int top = outlineCoordinates.Min(x => x.Y);
            for (int i = top + 1; i < bottom; i++)
            {
                IEnumerable<Coordinates> rowCords = outlineCoordinates.Where(x => x.Y == i);
                int right = rowCords.Max(x => x.X);
                int left = rowCords.Min(x => x.X);
                for (int j = left + 1; j < right; j++)
                {
                    layer.SetPixel(new Coordinates(j, i), color);
                }
            }
        }

        private List<Coordinates> DrawFallbackRectangle(Layer layer, SKColor color, double halfWidth, double halfHeight, double centerX, double centerY)
        {
            //using var ctx = layer.LayerBitmap.GetBitmapContext();

            List<Coordinates> coordinates = new List<Coordinates>();

            for (double x = centerX - halfWidth; x <= centerX + halfWidth; x++)
            {
                var cords = new Coordinates((int)x, (int)(centerY - halfHeight));
                coordinates.Add(cords);
                layer.SetPixel(cords, color);

                cords = new Coordinates((int)x, (int)(centerY + halfHeight));
                coordinates.Add(cords);
                layer.SetPixel(cords, color);
            }

            for (double y = centerY - halfHeight + 1; y <= centerY + halfHeight - 1; y++)
            {
                var cords = new Coordinates((int)(centerX - halfWidth), (int)y);
                coordinates.Add(cords);
                layer.SetPixel(cords, color);

                cords = new Coordinates((int)(centerX + halfWidth), (int)y);
                coordinates.Add(cords);
                layer.SetPixel(cords, color);
            }

            return coordinates;
        }

        private Coordinates[] DrawRegionPoints(Layer layer, SKColor color, double x, double xc, double y, double yc)
        {
            Coordinates[] outputCoordinates = new Coordinates[4];
            outputCoordinates[0] = new Coordinates((int)Math.Floor(x), (int)Math.Floor(y));
            layer.SetPixel(outputCoordinates[0], color);
            outputCoordinates[1] = new Coordinates((int)Math.Floor(-(x - xc) + xc), (int)Math.Floor(y));
            layer.SetPixel(outputCoordinates[1], color);
            outputCoordinates[2] = new Coordinates((int)Math.Floor(x), (int)Math.Floor(-(y - yc) + yc));
            layer.SetPixel(outputCoordinates[2], color);
            outputCoordinates[3] = new Coordinates((int)Math.Floor(-(x - xc) + xc), (int)Math.Floor(-(y - yc) + yc));
            layer.SetPixel(outputCoordinates[3], color);

            return outputCoordinates;
        }
    }
}
