using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels;
using Color = System.Windows.Media.Color;

namespace PixiEditor.Models.Images
{
    public static class BitmapUtils
    {
        public static WriteableBitmap BytesToWriteableBitmap(int currentBitmapWidth, int currentBitmapHeight,
            byte[] byteArray)
        {
            WriteableBitmap bitmap = BitmapFactory.New(currentBitmapWidth, currentBitmapHeight);
            bitmap.FromByteArray(byteArray);
            return bitmap;
        }

        public static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        public static WriteableBitmap CombineLayers(Layer[] layers)
        {
            int width = ViewModelMain.Current.BitmapManager.ActiveDocument.Width;
            int height = ViewModelMain.Current.BitmapManager.ActiveDocument.Height;

            WriteableBitmap finalBitmap = BitmapFactory.New(width, height);

            using (finalBitmap.GetBitmapContext())
            {
                for (int i = 0; i < layers.Length; i++)
                for (int y = 0; y < finalBitmap.Height; y++)
                for (int x = 0; x < finalBitmap.Width; x++)
                {
                    Color color = layers[i].GetPixelWithOffset(x, y);
                    color = Color.FromArgb((byte)(color.A * layers[i].Opacity), color.R,color.G, color.B);
                    if (color.A != 0 || color.R != 0 || color.B != 0 || color.G != 0) finalBitmap.SetPixel(x, y, color);
                }
            }

            return finalBitmap;
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
                            continue;
                        pixels[j] = layers[i].GetPixel(position.X, position.Y);
                    }
                }


                result[layers[i]] = pixels;
            }

            return result;
        }
    }
}