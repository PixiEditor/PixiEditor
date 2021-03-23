using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Parser;

namespace PixiEditor.Models.ImageManipulation
{
    public static class BitmapUtils
    {
        /// <summary>
        ///     Converts pixel bytes to WriteableBitmap.
        /// </summary>
        /// <param name="currentBitmapWidth">Width of bitmap.</param>
        /// <param name="currentBitmapHeight">Height of bitmap.</param>
        /// <param name="byteArray">Bitmap byte array.</param>
        /// <returns>WriteableBitmap.</returns>
        public static WriteableBitmap BytesToWriteableBitmap(int currentBitmapWidth, int currentBitmapHeight, byte[] byteArray)
        {
            WriteableBitmap bitmap = BitmapFactory.New(currentBitmapWidth, currentBitmapHeight);
            if (byteArray != null)
            {
                bitmap.FromByteArray(byteArray);
            }

            return bitmap;
        }

        /// <summary>
        ///     Converts layers bitmaps into one bitmap.
        /// </summary>
        /// <param name="layers">Layers to combine.</param>
        /// <param name="width">Width of final bitmap.</param>
        /// <param name="height">Height of final bitmap.</param>.
        /// <returns>WriteableBitmap of layered bitmaps.</returns>
        public static WriteableBitmap CombineLayers(int width, int height, params Layer[] layers)
        {
            WriteableBitmap finalBitmap = BitmapFactory.New(width, height);

            using (finalBitmap.GetBitmapContext())
            {
                for (int i = 0; i < layers.Length; i++)
                {
                    float layerOpacity = layers[i].Opacity;
                    Layer layer = layers[i];

                    for (int y = 0; y < layers[i].Height; y++)
                    {
                        for (int x = 0; x < layers[i].Width; x++)
                        {
                            Color color = layer.GetPixel(x, y);
                            if (i > 0 && ((color.A < 255 && color.A > 0) || (layerOpacity < 1f && layerOpacity > 0 && color.A > 0)))
                            {
                                var lastLayerPixel = finalBitmap.GetPixel(x + layer.OffsetX, y + layer.OffsetY);
                                byte pixelA = (byte)(color.A * layerOpacity);
                                byte r = (byte)((color.R * pixelA / 255) + (lastLayerPixel.R * lastLayerPixel.A * (255 - pixelA) / (255 * 255)));
                                byte g = (byte)((color.G * pixelA / 255) + (lastLayerPixel.G * lastLayerPixel.A * (255 - pixelA) / (255 * 255)));
                                byte b = (byte)((color.B * pixelA / 255) + (lastLayerPixel.B * lastLayerPixel.A * (255 - pixelA) / (255 * 255)));
                                byte a = (byte)(pixelA + (lastLayerPixel.A * (255 - pixelA) / 255));
                                color = Color.FromArgb(a, r, g, b);
                            }
                            else
                            {
                                color = Color.FromArgb(color.A, color.R, color.G, color.B);
                            }

                            if (color.A > 0)
                            {
                                finalBitmap.SetPixel(x + layer.OffsetX, y + layer.OffsetY, color);
                            }
                        }
                    }
                }
            }

            return finalBitmap;
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

        public static WriteableBitmap GeneratePreviewBitmap(IEnumerable<SerializableLayer> layers, int width, int height, int maxPreviewWidth, int maxPreviewHeight)
        {
            var opacityLayers = layers.Where(x => x.IsVisible && x.Opacity > 0.8f);

            return GeneratePreviewBitmap(
                opacityLayers.Select(x => BytesToWriteableBitmap(x.Width, x.Height, x.BitmapBytes)),
                opacityLayers.Select(x => x.OffsetX),
                opacityLayers.Select(x => x.OffsetY),
                width,
                height,
                maxPreviewWidth,
                maxPreviewHeight);
        }

        public static Dictionary<Guid, Color[]> GetPixelsForSelection(Layer[] layers, Coordinates[] selection)
        {
            Dictionary<Guid, Color[]> result = new ();

            foreach (Layer layer in layers)
            {
                Color[] pixels = new Color[selection.Length];

                using (layer.LayerBitmap.GetBitmapContext())
                {
                    for (int j = 0; j < pixels.Length; j++)
                    {
                        Coordinates position = layer.GetRelativePosition(selection[j]);
                        if (position.X < 0 || position.X > layer.Width - 1 || position.Y < 0 ||
                            position.Y > layer.Height - 1)
                        {
                            continue;
                        }

                        pixels[j] = layer.GetPixel(position.X, position.Y);
                    }
                }

                result[layer.LayerGuid] = pixels;
            }

            return result;
        }

        private static WriteableBitmap GeneratePreviewBitmap(
            IEnumerable<WriteableBitmap> layerBitmaps,
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
            return previewBitmap.Resize(newWidth, newHeight, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
        }
    }
}