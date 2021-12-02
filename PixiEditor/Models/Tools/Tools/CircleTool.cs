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
                paint.BlendMode = SKBlendMode.Src;

                paint.Color = color;
                paint.StrokeWidth = thickness;
                paint.Style = SKPaintStyle.Stroke;
                var outline = DrawEllipse(layer, color, centerX, centerY, radiusX, radiusY, thickness);

                if (hasFillColor)
                {
                    paint.Color = fillColor;
                    paint.Style = SKPaintStyle.StrokeAndFill;
                    DrawEllipseFill(layer, fillColor, outline);
                }

                // An Idea, use Skia DrawOval for bigger sizes.
            }

            layer.InvokeLayerBitmapChange(dirtyRect);
        }

        /// <summary>
        ///     Calculates ellipse points for specified coordinates and thickness.
        /// </summary>
        /// <param name="thickness">Thickness of ellipse.</param>
        public static IEnumerable<Coordinates> DrawEllipse(Layer layer, SKColor color, float centerX, float centerY, float radiusX, float radiusY, int thickness)
        {

            IEnumerable<Coordinates> ellipse = GenerateMidpointEllipse(layer, color, radiusX, radiusY, centerX, centerY);
            if (thickness > 1)
            {
                ShapeTool.ThickenShape(layer, color, ellipse, thickness);
            }

            return ellipse;
        }

        public static List<Coordinates> GenerateMidpointEllipse(Layer layer, SKColor color, double halfWidth, double halfHeight, double centerX, double centerY)
        {
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

        public static void DrawEllipseFill(Layer layer, SKColor color, IEnumerable<Coordinates> outlineCoordinates)
        {
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
                    layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(new SKPoint(j - layer.OffsetX, i - layer.OffsetY), color);
                }
            }
        }

        private static List<Coordinates> DrawFallbackRectangle(Layer layer, SKColor color, double halfWidth, double halfHeight, double centerX, double centerY)
        {
            List<Coordinates> coordinates = new List<Coordinates>();

            for (double x = centerX - halfWidth; x <= centerX + halfWidth; x++)
            {
                var cords = new Coordinates((int)x, (int)(centerY - halfHeight));
                coordinates.Add(cords);
                layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(new SKPoint(cords.X - layer.OffsetX, cords.Y - layer.OffsetY), color);

                cords = new Coordinates((int)x, (int)(centerY + halfHeight));
                coordinates.Add(cords);
                layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(new SKPoint(cords.X - layer.OffsetX, cords.Y - layer.OffsetY), color);

            }

            for (double y = centerY - halfHeight + 1; y <= centerY + halfHeight - 1; y++)
            {
                var cords = new Coordinates((int)(centerX - halfWidth), (int)y);
                coordinates.Add(cords);
                layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(new SKPoint(cords.X - layer.OffsetX, cords.Y - layer.OffsetY), color);

                cords = new Coordinates((int)(centerX + halfWidth), (int)y);
                coordinates.Add(cords);
                layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(new SKPoint(cords.X - layer.OffsetX, cords.Y - layer.OffsetY), color);

            }

            return coordinates;
        }

        private static Coordinates[] DrawRegionPoints(Layer layer, SKColor color, double x, double xc, double y, double yc)
        {
            Coordinates[] outputCoordinates = new Coordinates[4];
            outputCoordinates[0] = new Coordinates((int)Math.Floor(x), (int)Math.Floor(y));
            layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(new SKPoint(outputCoordinates[0].X - layer.OffsetX, outputCoordinates[0].Y - layer.OffsetY), color);
            outputCoordinates[1] = new Coordinates((int)Math.Floor(-(x - xc) + xc), (int)Math.Floor(y));
            layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(new SKPoint(outputCoordinates[1].X - layer.OffsetX, outputCoordinates[1].Y - layer.OffsetY), color);
            outputCoordinates[2] = new Coordinates((int)Math.Floor(x), (int)Math.Floor(-(y - yc) + yc));
            layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(new SKPoint(outputCoordinates[2].X - layer.OffsetX, outputCoordinates[2].Y - layer.OffsetY), color);
            outputCoordinates[3] = new Coordinates((int)Math.Floor(-(x - xc) + xc), (int)Math.Floor(-(y - yc) + yc));
            layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(new SKPoint(outputCoordinates[3].X - layer.OffsetX, outputCoordinates[3].Y - layer.OffsetY), color);

            return outputCoordinates;
        }
    }
}
