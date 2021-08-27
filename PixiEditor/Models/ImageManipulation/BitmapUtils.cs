using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Layers.Utils;
using PixiEditor.Models.Position;
using PixiEditor.Parser;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.ImageManipulation
{
    public static class BitmapUtils
    {
        public static Surface CombineLayers(int width, int height, IEnumerable<Layer> layers, LayerStructure structure = null)
        {
            Surface finalSurface = new(width, height);
            using SKPaint paint = new();

            for (int i = 0; i < layers.Count(); i++)
            {
                Layer layer = layers.ElementAt(i);
                float layerOpacity = structure == null ? layer.Opacity : LayerStructureUtils.GetFinalLayerOpacity(layer, structure);
                paint.Color = new(255, 255, 255, (byte)(layerOpacity * 255));

                if (layer.OffsetX < 0 || layer.OffsetY < 0 ||
                    layer.Width + layer.OffsetX > layer.MaxWidth ||
                    layer.Height + layer.OffsetY > layer.MaxHeight)
                {
                    throw new InvalidOperationException("Layers must not extend beyond canvas borders");
                }

                layer.LayerBitmap.SkiaSurface.Draw(finalSurface.SkiaSurface.Canvas, layer.OffsetX, layer.OffsetY, paint);
            }

            return finalSurface;
        }

        public static Color GetColorAtPointCombined(int x, int y, params Layer[] layers)
        {
            Color prevColor = Color.FromArgb(0, 0, 0, 0);
            /*
            for (int i = 0; i < layers.Length; i++)
            {
                Color color = layers[i].GetPixelWithOffset(x, y);
                float layerOpacity = layers[i].Opacity;

                prevColor = BlendColor(prevColor, color, layerOpacity);
            }*/

            return prevColor;
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
            var opacityLayers = layers.Where(x => x.IsVisible && x.Opacity > 0.8f);

            return GeneratePreviewBitmap(
                opacityLayers.Select(x => new Surface(x.Width, x.Height, x.BitmapBytes)),
                opacityLayers.Select(x => x.OffsetX),
                opacityLayers.Select(x => x.OffsetY),
                width,
                height,
                maxPreviewWidth,
                maxPreviewHeight);
        }

        public static Dictionary<Guid, Color[]> GetPixelsForSelection(Layer[] layers, Coordinates[] selection)
        {
            Dictionary<Guid, Color[]> result = new();

            foreach (Layer layer in layers)
            {
                Color[] pixels = new Color[selection.Length];

                for (int j = 0; j < pixels.Length; j++)
                {
                    Coordinates position = layer.GetRelativePosition(selection[j]);
                    if (position.X < 0 || position.X > layer.Width - 1 || position.Y < 0 ||
                        position.Y > layer.Height - 1)
                    {
                        continue;
                    }

                    var cl = layer.GetPixel(position.X, position.Y);
                    pixels[j] = Color.FromArgb(cl.Alpha, cl.Red, cl.Green, cl.Blue);
                }
                result[layer.LayerGuid] = pixels;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Color BlendColor(Color previousColor, Color color, float opacity)
        {
            if ((color.A < 255 && color.A > 0) || (opacity < 1f && opacity > 0 && color.A > 0))
            {
                byte pixelA = (byte)(color.A * opacity);
                byte r = (byte)((color.R * pixelA / 255) + (previousColor.R * previousColor.A * (255 - pixelA) / (255 * 255)));
                byte g = (byte)((color.G * pixelA / 255) + (previousColor.G * previousColor.A * (255 - pixelA) / (255 * 255)));
                byte b = (byte)((color.B * pixelA / 255) + (previousColor.B * previousColor.A * (255 - pixelA) / (255 * 255)));
                byte a = (byte)(pixelA + (previousColor.A * (255 - pixelA) / 255));
                color = Color.FromArgb(a, r, g, b);
            }
            else
            {
                color = Color.FromArgb(color.A, color.R, color.G, color.B);
            }

            if (color.A > 0)
            {
                return color;
            }

            return previousColor;
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

            using Surface previewSurface = new Surface(maxPreviewWidth, maxPreviewHeight);
            return previewSurface.ToWriteableBitmap();
            /*
            WriteableBitmap previewBitmap = BitmapFactory.New(width, height);

            var layerBitmapsEnumerator = layerBitmaps.GetEnumerator();
            var offsetsXEnumerator = offsetsX.GetEnumerator();
            var offsetsYEnumerator = offsetsY.GetEnumerator();

            while (layerBitmapsEnumerator.MoveNext())
            {
                offsetsXEnumerator.MoveNext();
                offsetsYEnumerator.MoveNext();

                var bitmap = layerBitmapsEnumerator.Current;
                var offsetX = offsetsXEnumerator.Current;
                var offsetY = offsetsYEnumerator.Current;

                previewBitmap.Blit(
                    new Rect(offsetX, offsetY, bitmap.Width, bitmap.Height),
                    bitmap,
                    new Rect(0, 0, bitmap.Width, bitmap.Height));
            }

            int newWidth = width >= height ? maxPreviewWidth : (int)Math.Ceiling(width / ((float)height / maxPreviewHeight));
            int newHeight = height > width ? maxPreviewHeight : (int)Math.Ceiling(height / ((float)width / maxPreviewWidth));
            return previewBitmap.Resize(newWidth, newHeight, WriteableBitmapExtensions.Interpolation.NearestNeighbor);*/
        }
    }
}
