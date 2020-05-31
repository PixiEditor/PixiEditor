using PixiEditor.Models.Layers;
using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Images
{
    public static class BitmapUtils
    {
        public static WriteableBitmap BytesToWriteableBitmap(int currentBitmapWidth, int currentBitmapHeight, byte[] byteArray)
        {
            WriteableBitmap bitmap = BitmapFactory.New(currentBitmapWidth, currentBitmapHeight);
            bitmap.FromByteArray(byteArray);
            return bitmap;
        }

        public static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                  bitmap.GetHbitmap(),
                  IntPtr.Zero,
                  Int32Rect.Empty,
                  BitmapSizeOptions.FromEmptyOptions());
        }

        public static WriteableBitmap CombineBitmaps(Layer[] layers)
        {
            return CombineBitmaps(layers.Select(x => x.LayerBitmap).ToArray());
        }

        public static WriteableBitmap CombineBitmaps(WriteableBitmap[] bitmaps)
        {
            WriteableBitmap finalBitmap = bitmaps[0].Clone();
            finalBitmap.Lock();
            for (int i = 1; i < bitmaps.Length; i++)
            {
                for (int y = 0; y < finalBitmap.Height; y++)
                {
                    for (int x = 0; x < finalBitmap.Width; x++)
                    {
                        System.Windows.Media.Color color = bitmaps[i].GetPixel(x, y);
                        if (color.A != 0 || color.R != 0 || color.B != 0 || color.G != 0)
                        {
                            finalBitmap.SetPixel(x, y, color);
                        }
                    }
                }
            }
            finalBitmap.Unlock();
            return finalBitmap;
        }
    }
}
