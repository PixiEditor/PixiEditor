using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using PixiEditor.Models.DataHolders;
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
        /// <param name="height">Height of final bitmap.</param>
        /// <param name="stepX">Width precision, determinates how much pixels per row to calculate.
        /// Ex. layer width = 500, stepX = 5, output image would be 50px wide. So to fill all space, width would need to be 50.</param>
        /// /// <param name="stepY">Height precision, determinates how much pixels per column to calculate.
        /// Ex. layer height = 500, stepY = 5, output image would be 50px high. So to fill all space, height would need to be 50.</param>
        /// <returns>WriteableBitmap of layered bitmaps.</returns>
        public static WriteableBitmap CombineLayers(Layer[] layers, int width, int height, int stepX = 1, int stepY = 1)
        {
            WriteableBitmap finalBitmap = BitmapFactory.New(width, height);

            using (finalBitmap.GetBitmapContext())
            {
                for (int i = 0; i < layers.Length; i++)
                {
                    float layerOpacity = layers[i].Opacity;
                    for (int y = 0; y < finalBitmap.Height; y += stepY)
                    {
                        for (int x = 0; x < finalBitmap.Width; x += stepX)
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

        public static WriteableBitmap GeneratePreviewBitmap(Document document, int maxPreviewWidth, int maxPreviewHeight)
        {
            int stepX = 1;
            int stepY = 1;
            int targetWidth = document.Width;
            int targetHeight = document.Height;
            if (document.Width > maxPreviewWidth || document.Height > maxPreviewHeight)
            {
                stepX = (int)Math.Floor((float)document.Width / maxPreviewWidth);
                stepY = (int)Math.Floor((float)document.Height / maxPreviewHeight);
                targetWidth = maxPreviewWidth;
                targetHeight = maxPreviewHeight;
            }

            return CombineLayers(document.Layers.ToArray(), document.Width, document.Height, stepX, stepY);
        }

        public static Dictionary<Layer, Color[]> GetPixelsForSelection(Layer[] layers, Coordinates[] selection)
        {
            Dictionary<Layer, Color[]> result = new Dictionary<Layer, Color[]>();

            for (int i = 0; i < layers.Length; i++)
            {
                Color[] pixels = new Color[selection.Length];

                using (layers[i].LayerBitmap.GetBitmapContext())
                {
                    for (int j = 0; j < pixels.Length; j++)
                    {
                        Coordinates position = layers[i].GetRelativePosition(selection[j]);
                        if (position.X < 0 || position.X > layers[i].Width - 1 || position.Y < 0 ||
                            position.Y > layers[i].Height - 1)
                        {
                            continue;
                        }

                        pixels[j] = layers[i].GetPixel(position.X, position.Y);
                    }
                }

                result[layers[i]] = pixels;
            }

            return result;
        }
    }
}