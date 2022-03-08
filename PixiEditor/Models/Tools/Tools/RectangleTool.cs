using PixiEditor.Helpers;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PixiEditor.Models.Tools.Tools
{
    public class RectangleTool : ShapeTool
    {
        private string defaultActionDisplay = "Click and move to draw a rectangle. Hold Shift to draw a square.";
        public RectangleTool()
        {
            ActionDisplay = defaultActionDisplay;
        }

        public override string Tooltip => $"Draws rectangle on canvas ({ShortcutKey}). Hold Shift to draw a square.";

        public bool Filled { get; set; } = false;

        public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
        {
            if (shiftIsDown)
                ActionDisplay = "Click and move to draw a square.";
            else
                ActionDisplay = defaultActionDisplay;
        }

        public override void Use(Layer activeLayer, Layer previewLayer, IEnumerable<Layer> allLayers, IReadOnlyList<Coordinates> recordedMouseMovement, SKColor color)
        {
            int thickness = Toolbar.GetSetting<SizeSetting>("ToolSize").Value;
            SKColor? fillColor = null;
            if (Toolbar.GetSetting<BoolSetting>("Fill").Value)
            {
                var temp = Toolbar.GetSetting<ColorSetting>("FillColor").Value;
                fillColor = new SKColor(temp.R, temp.G, temp.B, temp.A);
            }
            var dirtyRect = CreateRectangle(previewLayer, color, fillColor, recordedMouseMovement, thickness);
            ReportCustomSessionRect(SKRectI.Create(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height));
        }

        private Int32Rect CreateRectangle(Layer layer, SKColor color, SKColor? fillColor, IReadOnlyList<Coordinates> coordinates, int thickness)
        {
            var (start, end) = Session.IsShiftDown ? CoordinatesHelper.GetSquareCoordiantes(coordinates) : (coordinates[0], coordinates[^1]);

            DoubleCoords fixedCoordinates = CalculateCoordinatesForShapeRotation(start, end);

            int halfThickness = (int)Math.Ceiling(thickness / 2.0);
            Int32Rect dirtyRect = new Int32Rect(
                fixedCoordinates.Coords1.X - halfThickness,
                fixedCoordinates.Coords1.Y - halfThickness,
                fixedCoordinates.Coords2.X + halfThickness * 2 - fixedCoordinates.Coords1.X,
                fixedCoordinates.Coords2.Y + halfThickness * 2 - fixedCoordinates.Coords1.Y);
            layer.DynamicResizeAbsolute(dirtyRect);

            using (SKPaint paint = new SKPaint())
            {
                int x = fixedCoordinates.Coords1.X - layer.OffsetX;
                int y = fixedCoordinates.Coords1.Y - layer.OffsetY;
                int w = fixedCoordinates.Coords2.X - fixedCoordinates.Coords1.X;
                int h = fixedCoordinates.Coords2.Y - fixedCoordinates.Coords1.Y;
                paint.BlendMode = SKBlendMode.Src;

                if (fillColor.HasValue)
                {
                    paint.Color = fillColor.Value;
                    paint.Style = SKPaintStyle.StrokeAndFill;
                    layer.LayerBitmap.SkiaSurface.Canvas.DrawRect(x, y, w, h, paint);
                }

                paint.StrokeWidth = thickness;
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = color;
                layer.LayerBitmap.SkiaSurface.Canvas.DrawRect(x, y, w, h, paint);
            }

            layer.InvokeLayerBitmapChange(dirtyRect);
            return dirtyRect;
        }
    }
}
