using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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
            var hasFillColor = Toolbar.GetSetting<BoolSetting>("Fill").Value;
            Color temp = Toolbar.GetSetting<ColorSetting>("FillColor").Value;
            SKColor fill = new SKColor(temp.R, temp.G, temp.B, temp.A);
            DrawEllipseFromRect(layer, coordinates[^1], coordinates[0], color, fill, thickness, hasFillColor);
        }

        public static void DrawEllipseFromRect(Layer layer, Coordinates first, Coordinates second,
            SKColor color, SKColor fillColor, int thickness, bool hasFillColor)
        {
            DoubleCords fixedCoordinates = CalculateCoordinatesForShapeRotation(first, second);

            int halfThickness = (int)Math.Ceiling(thickness / 2.0);
            Int32Rect dirtyRect = new Int32Rect(
                fixedCoordinates.Coords1.X - halfThickness,
                fixedCoordinates.Coords1.Y - halfThickness,
                fixedCoordinates.Coords2.X + halfThickness * 2 - fixedCoordinates.Coords1.X,
                fixedCoordinates.Coords2.Y + halfThickness * 2 - fixedCoordinates.Coords1.Y);
            layer.DynamicResizeAbsolute(dirtyRect.X + dirtyRect.Width - 1, dirtyRect.Y + dirtyRect.Height - 1, dirtyRect.X, dirtyRect.Y);

            using (SKPaint paint = new SKPaint())
            {
                float radiusX = (fixedCoordinates.Coords2.X - fixedCoordinates.Coords1.X) / 2.0f;
                float radiusY = (fixedCoordinates.Coords2.Y - fixedCoordinates.Coords1.Y) / 2.0f;
                float centerX = (fixedCoordinates.Coords1.X + fixedCoordinates.Coords2.X + 1) / 2.0f;
                float centerY = (fixedCoordinates.Coords1.Y + fixedCoordinates.Coords2.Y + 1) / 2.0f;

                List<Coordinates> outline = GenerateMidpointEllipse(radiusX, radiusY, centerX, centerY);
                if (hasFillColor)
                {
                    DrawEllipseFill(layer, fillColor, outline);
                }
                DrawEllipseOutline(layer, color, outline, thickness);

                // An Idea, use Skia DrawOval for bigger sizes.
            }

            layer.InvokeLayerBitmapChange(dirtyRect);
        }

        public static void DrawEllipseFill(Layer layer, SKColor color, List<Coordinates> outlineCoordinates)
        {
            if (!outlineCoordinates.Any())
                return;

            int bottom = outlineCoordinates.Max(x => x.Y);
            int top = outlineCoordinates.Min(x => x.Y);

            using SKPaint fillPaint = new();
            fillPaint.Color = color;
            fillPaint.BlendMode = SKBlendMode.Src;

            for (int i = top + 1; i < bottom; i++)
            {
                IEnumerable<Coordinates> rowCords = outlineCoordinates.Where(x => x.Y == i);
                int right = rowCords.Max(x => x.X);
                int left = rowCords.Min(x => x.X);
                layer.LayerBitmap.SkiaSurface.Canvas.DrawLine(left - layer.OffsetX, i - layer.OffsetY, right - layer.OffsetX, i - layer.OffsetY, fillPaint);
            }
        }

        /// <summary>
        ///     Calculates ellipse points for specified coordinates and thickness.
        /// </summary>
        /// <param name="thickness">Thickness of ellipse.</param>
        public static void DrawEllipseOutline(Layer layer, SKColor color, List<Coordinates> ellipse, int thickness)
        {
            if (thickness == 1)
            {
                foreach (var coords in ellipse)
                {
                    layer.LayerBitmap.SetSRGBPixel(coords.X - layer.OffsetX, coords.Y - layer.OffsetY, color);
                }
            }
            else
            {
                using SKPaint paint = new();
                paint.Color = color;
                paint.BlendMode = SKBlendMode.Src;
                float offsetX = thickness % 2 == 1 ? layer.OffsetX - 0.5f : layer.OffsetX;
                float offsetY = thickness % 2 == 1 ? layer.OffsetY - 0.5f : layer.OffsetY;
                foreach (var coords in ellipse)
                {
                    layer.LayerBitmap.SkiaSurface.Canvas.DrawCircle(coords.X - offsetX, coords.Y - offsetY, thickness / 2f, paint);
                }
            }
        }

        public static List<Coordinates> GenerateMidpointEllipse(double halfWidth, double halfHeight, double centerX, double centerY)
        {
            if (halfWidth < 1 || halfHeight < 1)
            {
                return GenerateFallbackRectangle(halfWidth, halfHeight, centerX, centerY);
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
                AddRegionPoints(outputCoordinates, currentX, centerX, currentY, centerY);

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
                AddRegionPoints(outputCoordinates, currentX, centerX, currentY, centerY);

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

        private static List<Coordinates> GenerateFallbackRectangle(double halfWidth, double halfHeight, double centerX, double centerY)
        {
            List<Coordinates> coordinates = new List<Coordinates>();

            for (double x = centerX - halfWidth; x <= centerX + halfWidth; x++)
            {
                coordinates.Add(new Coordinates((int)x, (int)(centerY - halfHeight)));
                coordinates.Add(new Coordinates((int)x, (int)(centerY + halfHeight)));
            }

            for (double y = centerY - halfHeight + 1; y <= centerY + halfHeight - 1; y++)
            {
                coordinates.Add(new Coordinates((int)(centerX - halfWidth), (int)y));
                coordinates.Add(new Coordinates((int)(centerX + halfWidth), (int)y));
            }

            return coordinates;
        }

        private static void AddRegionPoints(List<Coordinates> coordinates, double x, double xc, double y, double yc)
        {
            coordinates.Add(new Coordinates((int)Math.Floor(x), (int)Math.Floor(y)));
            coordinates.Add(new Coordinates((int)Math.Floor(-(x - xc) + xc), (int)Math.Floor(y)));
            coordinates.Add(new Coordinates((int)Math.Floor(x), (int)Math.Floor(-(y - yc) + yc)));
            coordinates.Add(new Coordinates((int)Math.Floor(-(x - xc) + xc), (int)Math.Floor(-(y - yc) + yc)));
        }
    }
}
