using PixiEditor.Models.Controllers;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.Brushes;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Models.Tools.Tools
{
    public class PenTool : ShapeTool
    {
        private readonly SizeSetting toolSizeSetting;
        private readonly BoolSetting pixelPerfectSetting;
        private readonly List<Coordinates> confirmedPixels = new List<Coordinates>();
        private readonly LineTool lineTool;
        private SKPaint paint = new SKPaint() { Style = SKPaintStyle.Stroke };
        private Coordinates[] lastChangedPixels = new Coordinates[3];
        private byte changedPixelsindex;
        private Coordinates lastChangedPixel = new Coordinates(-1, -1);

        private BitmapManager BitmapManager { get; }

        private Dictionary<int, CustomBrush> _customBrushes = new Dictionary<int, CustomBrush>();

        private bool _drawCustomBrush = false;

        public PenTool(BitmapManager bitmapManager)
        {
            Cursor = Cursors.Pen;
            ActionDisplay = "Click and move to draw.";
            Toolbar = new PenToolbar();
            toolSizeSetting = Toolbar.GetSetting<SizeSetting>("ToolSize");
            pixelPerfectSetting = Toolbar.GetSetting<BoolSetting>("PixelPerfectEnabled");
            ClearPreviewLayerOnEachIteration = false;
            BitmapManager = bitmapManager;
            paint.BlendMode = SKBlendMode.Src;
            lineTool = new LineTool();
            lineTool.AutomaticallyResizeCanvas = AutomaticallyResizeCanvas;
            _customBrushes.Add(1, new PointBrush());
            _customBrushes.Add(3, new CrossBrush());
        }

        public override string Tooltip => "Standard brush. (B)";
        public override bool UsesShift => false;

        public bool AutomaticallyResizeCanvas { get; set; } = true;

        public override void OnRecordingLeftMouseDown(MouseEventArgs e)
        {
            base.OnRecordingLeftMouseDown(e);
            changedPixelsindex = 0;
            lastChangedPixels = new Coordinates[3];
            confirmedPixels.Clear();
        }

        public override void Use(Layer layer, List<Coordinates> coordinates, SKColor color)
        {
            Coordinates startingCords = coordinates.Count > 1 ? coordinates[1] : coordinates[0];
            if (AutomaticallyResizeCanvas)
            {
                layer.DynamicResizeAbsolute(coordinates.Max(x => x.X), coordinates.Max(x => x.Y), coordinates.Min(x => x.X), coordinates.Min(x => x.Y));
            }
            Draw(
                layer,
                startingCords,
                coordinates[0],
                color,
                toolSizeSetting.Value,
                pixelPerfectSetting.Value,
                BitmapManager.ActiveDocument.PreviewLayer);
        }

        public void Draw(
            Layer layer, Coordinates startingCoords, Coordinates latestCords, SKColor color, int toolSize,
            bool pixelPerfect = false,
            Layer previewLayer = null,
            SKBlendMode blendMode = SKBlendMode.Src)
        {

            SKStrokeCap cap = SKStrokeCap.Round;
            _drawCustomBrush = _customBrushes.ContainsKey(toolSize);
            CustomBrush customBrush = null;
            paint.Color = color;

            if (_drawCustomBrush)
            {
                cap = SKStrokeCap.Butt;
                customBrush = _customBrushes[toolSize];
            }

            if (!pixelPerfect)
            {
                if(_drawCustomBrush)
                {
                    DrawCustomBrush(layer, latestCords, customBrush);
                }

                lineTool.DrawLine(layer, startingCoords, latestCords, color, toolSize, blendMode, cap);
                return;
            }

            if (latestCords != lastChangedPixel)
            {
                if (previewLayer != null && previewLayer.GetPixelWithOffset(latestCords.X, latestCords.Y).Alpha > 0)
                {
                    confirmedPixels.Add(latestCords);
                }

                if (_drawCustomBrush)
                {
                    DrawCustomBrush(layer, latestCords, customBrush);
                }

                lineTool.DrawLine(layer, startingCoords, latestCords, color, toolSize, blendMode, cap);
                SetPixelToCheck(LineTool.GetBresenhamLine(startingCoords, latestCords));

                if (changedPixelsindex == 2)
                {
                    byte alpha = ApplyPixelPerfectToPixels(
                        layer,
                        lastChangedPixels[0],
                        lastChangedPixels[1],
                        lastChangedPixels[2],
                        color,
                        toolSize,
                        paint);

                    MovePixelsToCheck(alpha);

                    lastChangedPixel = latestCords;
                    return;
                }

                changedPixelsindex += changedPixelsindex >= 2 ? (byte)0 : (byte)1;
            }

            lastChangedPixel = latestCords;
        }

        private void DrawCustomBrush(Layer layer, Coordinates latestCords, CustomBrush customBrush)
        {
            layer.LayerBitmap.SkiaSurface.Canvas.DrawPoints(
                                    SKPointMode.Points,
                                    customBrush.GetAtPoint(latestCords, layer.OffsetX, layer.OffsetY),
                                    paint);
        }

        private void MovePixelsToCheck(byte alpha)
        {
            if (alpha != 0)
            {
                lastChangedPixels[0] = lastChangedPixels[1];
                lastChangedPixels[1] = lastChangedPixels[2];
                changedPixelsindex = 2;
            }
            else
            {
                lastChangedPixels[0] = lastChangedPixels[2];
                changedPixelsindex = 1;
            }
        }

        private void SetPixelToCheck(IEnumerable<Coordinates> latestPixels)
        {
            if (latestPixels.Count() == 1)
            {
                lastChangedPixels[changedPixelsindex] = latestPixels.First();
            }
            else
            {
                lastChangedPixels[changedPixelsindex] = latestPixels.ElementAt(1);
            }
        }

        private byte ApplyPixelPerfectToPixels(Layer layer, Coordinates p1, Coordinates p2, Coordinates p3, SKColor color, int toolSize, SKPaint paint)
        {
            byte alpha = color.Alpha;
            paint.StrokeWidth = toolSize;

            if (Math.Abs(p3.X - p1.X) == 1 && Math.Abs(p3.Y - p1.Y) == 1 && !confirmedPixels.Contains(p2))
            {
                paint.Color = SKColors.Transparent;
                layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(p2.X - layer.OffsetX, p2.Y - layer.OffsetY, paint);
                paint.Color = color;
                layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(p1.X - layer.OffsetX, p1.Y - layer.OffsetY, paint);
                layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(p3.X - layer.OffsetX, p3.Y - layer.OffsetY, paint);

                if (lastChangedPixels.Length > 1 && p2 == lastChangedPixels[1] /*Here might be a bug, I don't remember if it should be p2*/)
                {
                    alpha = 0;
                }
            }
            else
            {
                layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(p2.X - layer.OffsetX, p2.Y - layer.OffsetY, paint);
                layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(p3.X - layer.OffsetX, p3.Y - layer.OffsetY, paint);
            }

            Int32Rect dirtyRect = new Int32Rect(
               p2.X,
               p2.Y,
               2,
               2);

            layer.InvokeLayerBitmapChange(dirtyRect);
            return alpha;
        }

        internal void Draw(Layer previewLayer, object zero1, object zero2, SKColor highlightColor, int toolSize)
        {
            throw new NotImplementedException();
        }
    }
}
