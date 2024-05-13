using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.Helpers;

internal static class WriteableBitmapHelpers
{
    public static WriteableBitmap FromPbgra32Array(byte[] pbgra32pixels, VecI size)
    {
        WriteableBitmap bitmap = new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Pbgra32, null);
        bitmap.WritePixels(new(0, 0, size.X, size.Y), pbgra32pixels, bitmap.BackBufferStride, 0);
        return bitmap;
    }
}
