using PixiEditor.Helpers;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace PixiEditor.Models.Tools.Tools
{
    public class CircleTool : ShapeTool
    {
        private string defaultActionDisplay = "Click and move mouse to draw a circle. Hold Shift to draw an even one.";

        public CircleTool()
        {
            ActionDisplay = defaultActionDisplay;
        }

        public override string Tooltip => "Draws circle on canvas (C). Hold Shift to draw even circle.";

        public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
        {
            if (shiftIsDown)
                ActionDisplay = "Click and move mouse to draw an even circle.";
            else
                ActionDisplay = defaultActionDisplay;
        }

        public override void Use(Layer activeLayer, Layer previewLayer, IEnumerable<Layer> allLayers, IReadOnlyList<Coordinates> recordedMouseMovement, SKColor color)
        {
            int thickness = Toolbar.GetSetting<SizeSetting>("ToolSize").Value;
            var hasFillColor = Toolbar.GetSetting<BoolSetting>("Fill").Value;
            Color temp = Toolbar.GetSetting<ColorSetting>("FillColor").Value;
            SKColor fill = new SKColor(temp.R, temp.G, temp.B, temp.A);

            var (start, end) = Session.IsShiftDown ?
                CoordinatesHelper.GetSquareCoordiantes(recordedMouseMovement) :
                (recordedMouseMovement[0], recordedMouseMovement[^1]);

            DrawEllipseFromCoordinates(previewLayer, start, end, color, fill, thickness, hasFillColor, this);
        }

        public static void DrawEllipseFromCoordinates(Layer layer, Coordinates first, Coordinates second,
            SKColor color, SKColor fillColor, int thickness, bool hasFillColor)
        {
            DrawEllipseFromCoordinates(layer, first, second, color, fillColor, thickness, hasFillColor, null);
        }

        static void DrawEllipseFromCoordinates(Layer layer, Coordinates first, Coordinates second,
            SKColor color, SKColor fillColor, int thickness, bool hasFillColor, ShapeTool tool) 
        {
            DoubleCoords corners = CalculateCoordinatesForShapeRotation(first, second);
            corners.Coords2 = new(corners.Coords2.X, corners.Coords2.Y);

            int halfThickness = (int)Math.Ceiling(thickness / 2.0);
            Int32Rect dirtyRect = new Int32Rect(
                corners.Coords1.X - halfThickness,
                corners.Coords1.Y - halfThickness,
                corners.Coords2.X + halfThickness * 2 - corners.Coords1.X,
                corners.Coords2.Y + halfThickness * 2 - corners.Coords1.Y);

            if(tool != null)
                dirtyRect = tool.ApplyDirtyRect(layer, dirtyRect);
            else
                dirtyRect = DoApplyDirtyRect(layer, dirtyRect);

            using (SKPaint paint = new SKPaint())
            {
                List<Coordinates> outline = EllipseGenerator.GenerateEllipseFromRect(corners);
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
    }
}
