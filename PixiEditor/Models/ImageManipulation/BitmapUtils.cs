using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Layers.Utils;
using PixiEditor.Models.Position;
using PixiEditor.Parser;
using PixiEditor.Parser.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.ImageManipulation
{
    public static class BitmapUtils
    {
        public static Surface CombineLayers(Int32Rect portion, IEnumerable<Layer> layers, LayerStructure structure = null)
        {
            Surface finalSurface = new(portion.Width, portion.Height);
            using SKPaint paint = new();

            for (int i = 0; i < layers.Count(); i++)
            {
                Layer layer = layers.ElementAt(i);
                if (structure != null && !LayerStructureUtils.GetFinalLayerIsVisible(layer, structure))
                    continue;
                float layerOpacity = structure == null ? layer.Opacity : LayerStructureUtils.GetFinalLayerOpacity(layer, structure);
                paint.Color = new(255, 255, 255, (byte)(layerOpacity * 255));

                if (layer.OffsetX < 0 || layer.OffsetY < 0 ||
                    layer.Width + layer.OffsetX > layer.MaxWidth ||
                    layer.Height + layer.OffsetY > layer.MaxHeight)
                {
                    throw new InvalidOperationException("Layers must not extend beyond canvas borders");
                }

                using SKImage snapshot = layer.LayerBitmap.SkiaSurface.Snapshot();
                int x = portion.X - layer.OffsetX;
                int y = portion.Y - layer.OffsetY;
                finalSurface.SkiaSurface.Canvas.DrawImage(
                    snapshot,
                    new SKRect(x, y, portion.Width + x, portion.Height + y),
                    new SKRect(0, 0, portion.Width, portion.Height),
                    paint);
            }

            return finalSurface;
        }

        public static Surface[] ExtractSelectedPortions(Layer selLayer, Layer[] extractFrom, bool eraceFromLayers)
        {
            using var selSnap = selLayer.LayerBitmap.SkiaSurface.Snapshot();
            Surface[] output = new Surface[extractFrom.Length];

            int count = 0;
            foreach (Layer layer in extractFrom)
            {
                Surface portion = new Surface(selLayer.Width, selLayer.Height);
                SKRect selLayerRect = new SKRect(0, 0, selLayer.Width, selLayer.Height);

                int x = selLayer.OffsetX - layer.OffsetX;
                int y = selLayer.OffsetY - layer.OffsetY;

                using (var layerSnap = layer.LayerBitmap.SkiaSurface.Snapshot())
                    portion.SkiaSurface.Canvas.DrawImage(layerSnap, new SKRect(x, y, x + selLayer.Width, y + selLayer.Height), selLayerRect, Surface.ReplacingPaint);
                portion.SkiaSurface.Canvas.DrawImage(selSnap, 0, 0, Surface.MaskingPaint);
                output[count] = portion;
                count++;

                if (eraceFromLayers)
                {
                    layer.LayerBitmap.SkiaSurface.Canvas.DrawImage(selSnap, new SKRect(0, 0, selLayer.Width, selLayer.Height),
                        new SKRect(selLayer.OffsetX - layer.OffsetX, selLayer.OffsetY - layer.OffsetY, selLayer.OffsetX - layer.OffsetX + selLayer.Width, selLayer.OffsetY - layer.OffsetY + selLayer.Height),
                        Surface.InverseMaskingPaint);
                    layer.InvokeLayerBitmapChange(new Int32Rect(selLayer.OffsetX, selLayer.OffsetY, selLayer.Width, selLayer.Height));
                }
            }
            return output;
        }

        /// <summary>
        /// Generates simplified preview from Document, very fast, great for creating small previews. Creates uniform streched image.
        /// </summary>
        /// <param name="document">Document which be used to generate preview.</param>
        /// <param name="maxPreviewWidth">Max width of preview.</param>
        /// <param name="maxPreviewHeight">Max height of preview.</param>
        /// <returns>WriteableBitmap image.</returns>
        public static WriteableBitmap GeneratePreviewBitmap(Document document, int maxPreviewWidth, int maxPreviewHeight)
        {
            var opacityLayers = document.Layers.Where(x => x.IsVisible && x.Opacity > 0.8f);

            return GeneratePreviewBitmap(
                opacityLayers.Select(x => x.LayerBitmap),
                opacityLayers.Select(x => x.OffsetX),
                opacityLayers.Select(x => x.OffsetY),
                document.Width,
                document.Height,
                maxPreviewWidth,
                maxPreviewHeight);
        }

        public static WriteableBitmap GeneratePreviewBitmap(IEnumerable<Layer> layers, int width, int height, int maxPreviewWidth, int maxPreviewHeight)
        {
            var opacityLayers = layers.Where(x => x.IsVisible && x.Opacity > 0.8f);

            return GeneratePreviewBitmap(
                opacityLayers.Select(x => x.LayerBitmap),
                opacityLayers.Select(x => x.OffsetX),
                opacityLayers.Select(x => x.OffsetY),
                width,
                height,
                maxPreviewWidth,
                maxPreviewHeight);
        }

        public static WriteableBitmap GeneratePreviewBitmap(IEnumerable<SerializableLayer> layers, int width, int height, int maxPreviewWidth, int maxPreviewHeight)
        {
            var opacityLayers = layers.Where(x => x.IsVisible && x.Opacity > 0.8f
            && x.Height > 0 && x.Width > 0);

            return GeneratePreviewBitmap(
                opacityLayers.Select(x => new Surface(x.ToSKImage())),
                opacityLayers.Select(x => x.OffsetX),
                opacityLayers.Select(x => x.OffsetY),
                width,
                height,
                maxPreviewWidth,
                maxPreviewHeight);
        }

        public static Dictionary<Guid, SKColor[]> GetPixelsForSelection(Layer[] layers, Coordinates[] selection)
        {
            Dictionary<Guid, SKColor[]> result = new();

            foreach (Layer layer in layers)
            {
                SKColor[] pixels = new SKColor[selection.Length];

                for (int j = 0; j < pixels.Length; j++)
                {
                    Coordinates position = layer.GetRelativePosition(selection[j]);
                    if (position.X < 0 || position.X > layer.Width - 1 || position.Y < 0 ||
                        position.Y > layer.Height - 1)
                    {
                        continue;
                    }

                    var cl = layer.GetPixel(position.X, position.Y);
                    pixels[j] = cl;
                }
                result[layer.GuidValue] = pixels;
            }

            return result;
        }

        public static SKColor BlendColors(SKColor bottomColor, SKColor topColor)
        {
            if ((topColor.Alpha < 255 && topColor.Alpha > 0))
            {
                byte r = (byte)((topColor.Red * topColor.Alpha / 255) + (bottomColor.Red * bottomColor.Alpha * (255 - topColor.Alpha) / (255 * 255)));
                byte g = (byte)((topColor.Green * topColor.Alpha / 255) + (bottomColor.Green * bottomColor.Alpha * (255 - topColor.Alpha) / (255 * 255)));
                byte b = (byte)((topColor.Blue * topColor.Alpha / 255) + (bottomColor.Blue * bottomColor.Alpha * (255 - topColor.Alpha) / (255 * 255)));
                byte a = (byte)(topColor.Alpha + (bottomColor.Alpha * (255 - topColor.Alpha) / 255));
                return new SKColor(r, g, b, a);
            }

            return topColor.Alpha == 255 ? topColor : bottomColor;
        }

        private static WriteableBitmap GeneratePreviewBitmap(
            IEnumerable<Surface> layerBitmaps,
            IEnumerable<int> offsetsX,
            IEnumerable<int> offsetsY,
            int width,
            int height,
            int maxPreviewWidth,
            int maxPreviewHeight)
        {
            int count = layerBitmaps.Count();

            if (count != offsetsX.Count() || count != offsetsY.Count())
            {
                throw new ArgumentException("There were not the same amount of bitmaps and offsets", nameof(layerBitmaps));
            }

            using Surface previewSurface = new Surface(width, height);

            var layerBitmapsEnumerator = layerBitmaps.GetEnumerator();
            var offsetsXEnumerator = offsetsX.GetEnumerator();
            var offsetsYEnumerator = offsetsY.GetEnumerator();

            while (layerBitmapsEnumerator.MoveNext())
            {
                offsetsXEnumerator.MoveNext();
                offsetsYEnumerator.MoveNext();

                var bitmap = layerBitmapsEnumerator.Current.SkiaSurface.Snapshot();
                var offsetX = offsetsXEnumerator.Current;
                var offsetY = offsetsYEnumerator.Current;

                previewSurface.SkiaSurface.Canvas.DrawImage(
                    bitmap,
                    offsetX, offsetY, Surface.BlendingPaint);
            }

            int newWidth = width >= height ? maxPreviewWidth : (int)Math.Ceiling(width / ((float)height / maxPreviewHeight));
            int newHeight = height > width ? maxPreviewHeight : (int)Math.Ceiling(height / ((float)width / maxPreviewWidth));
            return previewSurface.ResizeNearestNeighbor(newWidth, newHeight).ToWriteableBitmap();
        }
    }
}
