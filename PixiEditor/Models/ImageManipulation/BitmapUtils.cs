using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using Color = System.Windows.Media.Color;

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
            bitmap.FromByteArray(byteArray);
            return bitmap;
        }

        /// <summary>
        ///     Converts layers bitmaps into one bitmap.
        /// </summary>
        /// <param name="layers">Layers to combine.</param>
        /// <param name="width">Width of final bitmap.</param>
        /// <param name="height">Height of final bitmap.</param>
        /// <returns>WriteableBitmap of layered bitmaps.</returns>
        public static WriteableBitmap CombineLayers(Layer[] layers, int width, int height)
        {
            WriteableBitmap finalBitmap = BitmapFactory.New(width, height);

            using (finalBitmap.GetBitmapContext())
            {
                for (int i = 0; i < layers.Length; i++)
                {
                    float layerOpacity = layers[i].Opacity;
                    for (int y = 0; y < finalBitmap.Height; y++)
                    {
                        for (int x = 0; x < finalBitmap.Width; x++)
                        {
                            Color color = layers[i].GetPixelWithOffset(x, y);
                            if (i > 0 && ((color.A < 255 && color.A > 0) || (layerOpacity < 1f && layerOpacity > 0 && color.A > 0)))
                            {
                                var lastLayerPixel = finalBitmap.GetPixel(x, y);
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
                                finalBitmap.SetPixel(x, y, color);
                            }
                        }
                    }
                }
            }

            return finalBitmap;
        }

        public static Dictionary<Guid, Color[]> GetPixelsForSelection(IEnumerable<Layer> layers, Coordinates[] selection)
        {
            Dictionary<Guid, Color[]> result = new Dictionary<Guid, Color[]>();

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
    }
}