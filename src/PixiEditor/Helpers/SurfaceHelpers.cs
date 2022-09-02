using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using SkiaSharp;

namespace PixiEditor.Helpers;

public static class SurfaceHelpers
{
    public static Surface FromBitmapSource(BitmapSource original)
    {
        SKColorType color = original.Format.ToSkia(out SKAlphaType alpha);
        if (original.PixelWidth <= 0 || original.PixelHeight <= 0)
            throw new ArgumentException("Surface dimensions must be non-zero");

        int stride = (original.PixelWidth * original.Format.BitsPerPixel + 7) / 8;
        byte[] pixels = new byte[stride * original.PixelHeight];
        original.CopyPixels(pixels, stride, 0);

        Surface surface = new Surface(new VecI(original.PixelWidth, original.PixelHeight));
        surface.DrawBytes(surface.Size, pixels, color, alpha);
        return surface;
    }

    public static WriteableBitmap ToWriteableBitmap(this Surface surface)
    {
        int width = surface.Size.X;
        int height = surface.Size.Y;
        WriteableBitmap result = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
        result.Lock();
        var dirty = new Int32Rect(0, 0, width, height);
        result.WritePixels(dirty, ToByteArray(surface), width * 4, 0);
        result.AddDirtyRect(dirty);
        result.Unlock();
        return result;
    }

    private static unsafe byte[] ToByteArray(Surface surface, SKColorType colorType = SKColorType.Bgra8888, SKAlphaType alphaType = SKAlphaType.Premul)
    {
        int width = surface.Size.X;
        int height = surface.Size.Y;
        var imageInfo = new SKImageInfo(width, height, colorType, alphaType, SKColorSpace.CreateSrgb());

        byte[] buffer = new byte[width * height * imageInfo.BytesPerPixel];
        fixed (void* pointer = buffer)
        {
            if (!surface.DrawingSurface.ReadPixels(imageInfo, new IntPtr(pointer), imageInfo.RowBytes, 0, 0))
            {
                throw new InvalidOperationException("Could not read surface into buffer");
            }
        }

        return buffer;
    }
}
