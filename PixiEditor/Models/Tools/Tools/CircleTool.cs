using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using SkiaSharp;
using System;
using System.Collections.Generic;
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
            DoubleCords fixedCoordinates = CalculateCoordinatesForShapeRotation(coordinates[^1], coordinates[0]);

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
                float centerX = (fixedCoordinates.Coords1.X + fixedCoordinates.Coords2.X + 1) / 2.0f - layer.OffsetX;
                float centerY = (fixedCoordinates.Coords1.Y + fixedCoordinates.Coords2.Y + 1) / 2.0f - layer.OffsetY;

                if (hasFillColor)
                {
                    Color temp = Toolbar.GetSetting<ColorSetting>("FillColor").Value;
                    SKColor fillColor = new SKColor(temp.R, temp.G, temp.B, temp.A);

                    paint.Color = fillColor;
                    paint.Style = SKPaintStyle.StrokeAndFill;
                    layer.LayerBitmap.SkiaSurface.Canvas.DrawOval(centerX, centerY, radiusX, radiusY, paint);
                }

                paint.Color = color;
                paint.StrokeWidth = thickness;
                paint.Style = SKPaintStyle.Stroke;
                layer.LayerBitmap.SkiaSurface.Canvas.DrawOval(centerX, centerY, radiusX, radiusY, paint);
            }

            layer.InvokeLayerBitmapChange(dirtyRect);

        }
    }
}
