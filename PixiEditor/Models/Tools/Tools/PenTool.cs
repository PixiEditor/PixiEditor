﻿using PixiEditor.Models.Controllers;
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
    internal class PenTool : ShapeTool
    {
        public Brush Brush { get; set; }
        public List<Brush> Brushes { get; } = new List<Brush>();


        private readonly SizeSetting toolSizeSetting;
        private readonly BoolSetting pixelPerfectSetting;
        private readonly List<Coordinates> confirmedPixels = new List<Coordinates>();
        private readonly LineTool lineTool;
        private SKPaint paint = new SKPaint() { Style = SKPaintStyle.Stroke };
        private Coordinates[] lastChangedPixels = new Coordinates[3];
        private byte changedPixelsindex;
        private Coordinates lastChangedPixel = new Coordinates(-1, -1);

        private BitmapManager BitmapManager { get; }


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
            Brushes.Add(new CircleBrush());
            Brush = Brushes[0];
            lineTool = new LineTool
            {
                AutomaticallyResizeCanvas = AutomaticallyResizeCanvas
            };
        }

        public override string Tooltip => "Standard brush. (B)";

        public bool AutomaticallyResizeCanvas { get; set; } = true;

        public override void BeforeUse()
        {
            base.BeforeUse();
            changedPixelsindex = 0;
            lastChangedPixels = new Coordinates[] { new(-1, -1), new(-1, -1), new(-1, -1) };
            lastChangedPixel = new(-1, -1);
            confirmedPixels.Clear();
        }

        public override void Use(Layer activeLayer, Layer previewLayer, IEnumerable<Layer> allLayers, IReadOnlyList<Coordinates> recordedMouseMovement, SKColor color)
        {
            Coordinates startingCords = recordedMouseMovement.Count > 1 ? recordedMouseMovement[^2] : recordedMouseMovement[0];
            paint.Color = color;
            if (AutomaticallyResizeCanvas)
            {
                int maxX = recordedMouseMovement.Max(x => x.X);
                int maxY = recordedMouseMovement.Max(x => x.Y);
                int minX = recordedMouseMovement.Min(x => x.X);
                int minY = recordedMouseMovement.Min(x => x.Y);
                previewLayer.DynamicResizeAbsolute(new(minX, minY, maxX - minX + 1, maxX - minX + 1));
            }
            Draw(
                previewLayer,
                startingCords,
                recordedMouseMovement[^1],
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

            SKStrokeCap cap = SKStrokeCap.Butt;
            paint.Color = color;

            if (!pixelPerfect)
            {
                Brush.Draw(layer, toolSize, latestCords, paint);
                lineTool.DrawLine(layer, startingCoords, latestCords, color, toolSize, blendMode, cap);
                return;
            }

            if (latestCords != lastChangedPixel)
            {
                if (previewLayer != null && previewLayer.GetPixelWithOffset(latestCords.X, latestCords.Y).Alpha > 0)
                {
                    confirmedPixels.Add(latestCords);
                }

                Brush.Draw(layer, toolSize, latestCords, paint);

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
    }
}
