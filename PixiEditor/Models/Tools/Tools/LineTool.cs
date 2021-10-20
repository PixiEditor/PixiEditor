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
        private SKPaint paint = new SKPaint() { Style = SKPaintStyle.Stroke };

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
            layer.DynamicResizeAbsolute(dirtyRect.X + dirtyRect.Width - 1, dirtyRect.Y + dirtyRect.Height - 1, dirtyRect.X, dirtyRect.Y);

            x -= layer.OffsetX;
            y -= layer.OffsetY;
            x1 -= layer.OffsetX;
            y1 -= layer.OffsetY;

            paint.StrokeWidth = thickness;
            paint.Color = color;
            paint.BlendMode = blendMode;
            paint.StrokeCap = strokeCap;

            if (thickness == 1)
            {
                BresenhamLine(layer, x, y, x1, y1, paint);
            }
            else
            {
                layer.LayerBitmap.SkiaSurface.Canvas.DrawLine(x, y, x1, y1, paint);
            }

            layer.InvokeLayerBitmapChange(dirtyRect);
        }

        private void BresenhamLine(Layer layer, int x1, int y1, int x2, int y2, SKPaint paint)
        {
            if (x1 == x2 && y1 == y2)
            {
                layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(x1, y1, paint);
            }

            int d, dx, dy, ai, bi, xi, yi;
            int x = x1, y = y1;

            if (x1 < x2)
            {
                xi = 1;
                dx = x2 - x1;
            }
            else
            {
                xi = -1;
                dx = x1 - x2;
            }

            if (y1 < y2)
            {
                yi = 1;
                dy = y2 - y1;
            }
            else
            {
                yi = -1;
                dy = y1 - y2;
            }

            layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(x, y, paint);

            if (dx > dy)
            {
                ai = (dy - dx) * 2;
                bi = dy * 2;
                d = bi - dx;

                while (x != x2)
                {
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        x += xi;
                    }

                    layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(x, y, paint);
                }
            }
            else
            {
                ai = (dx - dy) * 2;
                bi = dx * 2;
                d = bi - dy;

                while (y != y2)
                {
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        y += yi;
                    }

                    layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(x, y, paint);
                }
            }
        }
    }
}