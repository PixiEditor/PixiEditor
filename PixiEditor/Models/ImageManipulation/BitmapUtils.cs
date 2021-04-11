using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;

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
        /// <param name="width">Width of final bitmap.</param>
        /// <param name="height">Height of final bitmap.</param>.
        /// <param name="layers">Layers to combine.</param>
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
        /// <param name="document">Document which will be used to generate preview.</param>
        /// <param name="maxPreviewWidth">Max width of preview.</param>
        /// <param name="maxPreviewHeight">Max height of preview.</param>
        /// <returns>WriteableBitmap image.</returns>
        public static WriteableBitmap GeneratePreviewBitmap(Document document, int maxPreviewWidth, int maxPreviewHeight)
        {
            return GeneratePreviewBitmap(document.Layers, maxPreviewWidth, maxPreviewHeight, document.Width, document.Height, 0, 0);
        }

        /// <summary>
        /// Generates simplified preview from layers, very fast, great for creating small previews. Creates uniform streched image.
        /// </summary>
        /// <param name="layers">Layers which will be used to generate preview.</param>
        /// <param name="maxPreviewWidth">Max width of preview.</param>
        /// <param name="maxPreviewHeight">Max height of preview.</param>
        /// <returns>WriteableBitmap image.</returns>
        public static WriteableBitmap GeneratePreviewBitmap(IEnumerable<Layer> layers, int maxPreviewWidth, int maxPreviewHeight, bool showHidden = false)
        {
            int minOffsetX = layers.Min(x => x.OffsetX);
            int minOffsetY = layers.Min(x => x.OffsetY);
            int width = layers.Max(x => x.OffsetX + x.Width) - minOffsetX;
            int height = layers.Max(x => x.OffsetY + x.Height) - minOffsetY;
            return GeneratePreviewBitmap(layers, maxPreviewWidth, maxPreviewHeight, width, height, minOffsetX, minOffsetY, showHidden);
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

        private static WriteableBitmap GeneratePreviewBitmap(IEnumerable<Layer> layers, int maxPreviewWidth, int maxPreviewHeight, int width, int height, int minOffsetX, int minOffsetY, bool showHidden = false)
        {
            WriteableBitmap previewBitmap = BitmapFactory.New(width, height);

            // 0.8 because blit doesn't take into consideration layer opacity. Small images with opacity > 80% are simillar enough.
            foreach (var layer in layers.Where(x => (x.IsVisible || showHidden) && x.Opacity > 0.8f))
            {
                previewBitmap.Blit(
                    new Rect(layer.OffsetX - minOffsetX, layer.OffsetY - minOffsetY, layer.Width, layer.Height),
                    layer.LayerBitmap,
                    new Rect(0, 0, layer.Width, layer.Height));
            }

            int finalWidth = width >= height ? maxPreviewWidth : (int)Math.Ceiling(width / ((float)height / maxPreviewHeight));
            int finalHeight = height > width ? maxPreviewHeight : (int)Math.Ceiling(height / ((float)width / maxPreviewWidth));

            return previewBitmap.Resize(width, height, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
        }
    }
}