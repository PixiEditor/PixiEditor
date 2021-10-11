using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Models.Tools.Tools
{
    public class LineTool : ShapeTool
    {
        private readonly CircleTool circleTool;
        private List<Coordinates> linePoints = new List<Coordinates>();

        public LineTool()
        {
            ActionDisplay = "Click and move to draw a line. Hold Shift to draw an even one.";
            Toolbar = new BasicToolbar();
            circleTool = new CircleTool();
        }

        public override string Tooltip => "Draws line on canvas (L). Hold Shift to draw even line.";

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                ActionDisplay = "Click and move mouse to draw an even line.";
            }
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                ActionDisplay = "Click and move to draw a line. Hold Shift to draw an even one.";
            }
        }

        public override void Use(Layer layer, List<Coordinates> coordinates, SKColor color)
        {
            int thickness = Toolbar.GetSetting<SizeSetting>("ToolSize").Value;

            Coordinates start = coordinates[0];
            Coordinates end = coordinates[^1];

            DrawLine(layer, start, end, color, thickness, SKBlendMode.SrcOver);
        }

        public void DrawLine(
            Layer layer, Coordinates start, Coordinates end, SKColor color, int thickness, SKBlendMode blendMode,
            SKStrokeCap strokeCap = SKStrokeCap.Butt)
        {
            int x = start.X;
            int y = start.Y;
            int x1 = end.X;
            int y1 = end.Y;

            int dirtyX = Math.Min(x, x1) - thickness;
            int dirtyY = Math.Min(y, y1) - thickness;

            Int32Rect dirtyRect = new Int32Rect(
                dirtyX,
                dirtyY,
                Math.Max(x1, x) + thickness - dirtyX,
                Math.Max(y1, y) + thickness - dirtyY);
            Int32Rect curLayerRect = new(layer.OffsetX, layer.OffsetY, layer.Width, layer.Height);
            Int32Rect expanded = dirtyRect.Expand(curLayerRect);

            layer.DynamicResize(expanded.X + expanded.Width - 1, expanded.Y + expanded.Height - 1, expanded.X, expanded.Y);

            using (SKPaint paint = new SKPaint())
            {
                paint.StrokeWidth = thickness;
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = color;
                paint.BlendMode = blendMode;
                paint.StrokeCap = strokeCap;
                layer.LayerBitmap.SkiaSurface.Canvas.DrawLine(x, y, x1, y1, paint);
            }

            layer.InvokeLayerBitmapChange(dirtyRect);
        }
    }
}
