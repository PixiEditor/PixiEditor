using PixiEditor.Helpers;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PixiEditor.Models.Tools.Tools
{
    public class LineTool : ShapeTool
    {
        private List<Coordinates> linePoints = new List<Coordinates>();
        private SKPaint paint = new SKPaint() { Style = SKPaintStyle.Stroke };

        public bool AutomaticallyResizeCanvas { get; set; } = true;

        private string defaltActionDisplay = "Click and move to draw a line. Hold Shift to draw an even one.";

        public LineTool()
        {
            ActionDisplay = defaltActionDisplay;
            Toolbar = new BasicToolbar();
        }

        public override string Tooltip => $"Draws line on canvas ({ShortcutKey}). Hold Shift to draw even line.";

        public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
        {
            if (shiftIsDown)
                ActionDisplay = "Click and move mouse to draw an even line.";
            else
                ActionDisplay = defaltActionDisplay;
        }

        public override void Use(Layer activeLayer, Layer previewLayer, IEnumerable<Layer> allLayers, IReadOnlyList<Coordinates> recordedMouseMovement, SKColor color)
        {
            int thickness = Toolbar.GetSetting<SizeSetting>("ToolSize").Value;

            Coordinates start = recordedMouseMovement[0];
            Coordinates end = recordedMouseMovement[^1];

            if (Session.IsShiftDown)
                (start, end) = CoordinatesHelper.GetSquareOrLineCoordinates(recordedMouseMovement);

            DrawLine(previewLayer, start, end, color, thickness, SKBlendMode.Src);
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
            if (AutomaticallyResizeCanvas)
            {
                layer.DynamicResizeAbsolute(dirtyRect);
            }

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
                DrawBresenhamLine(layer, x, y, x1, y1, paint);
            }
            else
            {
                layer.LayerBitmap.SkiaSurface.Canvas.DrawLine(x, y, x1, y1, paint);
            }

            layer.InvokeLayerBitmapChange(dirtyRect);
        }

        private void DrawBresenhamLine(Layer layer, int x1, int y1, int x2, int y2, SKPaint paint)
        {
            if (x1 == x2 && y1 == y2)
            {
                layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(x1, y1, paint);
                return;
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


        public static List<Coordinates> GetBresenhamLine(Coordinates start, Coordinates end)
        {
            List<Coordinates> output = new List<Coordinates>();
            CalculateBresenhamLine(start, end, output);
            return output;
        }

        public static void CalculateBresenhamLine(Coordinates start, Coordinates end, List<Coordinates> output)
        {
            int x1 = start.X;
            int x2 = end.X;
            int y1 = start.Y;
            int y2 = end.Y;

            if (x1 == x2 && y1 == y2)
            {
                output.Add(start);
                return;
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

            output.Add(new Coordinates(x, y));

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

                    output.Add(new Coordinates(x, y));
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

                    output.Add(new Coordinates(x, y));
                }
            }
        }
    }
}
