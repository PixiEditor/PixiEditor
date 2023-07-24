using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ChunkyImageLib;
using PixiEditor.Avalonia.Helpers;
using PixiEditor.Avalonia.Helpers.Extensions;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.Helpers;

public static class SurfaceHelpers
{
    /*public static Surface FromBitmapSource(BitmapSource original)
    {
        ColorType color = original.Format.ToColorType(out AlphaType alpha);
        if (original.PixelWidth <= 0 || original.PixelHeight <= 0)
            throw new ArgumentException("Surface dimensions must be non-zero");

        int stride = (original.PixelWidth * original.Format.BitsPerPixel + 7) / 8;
        byte[] pixels = new byte[stride * original.PixelHeight];
        original.CopyPixels(pixels, stride, 0);

        Surface surface = new Surface(new VecI(original.PixelWidth, original.PixelHeight));
        surface.DrawBytes(surface.Size, pixels, color, alpha);
        return surface;
    }*/

    public static WriteableBitmap ToWriteableBitmap(this Surface surface)
    {
        WriteableBitmap result = WriteableBitmapHelpers.CreateBitmap(surface.Size);
        using var framebuffer = result.Lock();
        var dirty = new RectI(0, 0, surface.Size.X, surface.Size.Y);
        framebuffer.WritePixels(dirty, ToByteArray(surface));
        //result.AddDirtyRect(dirty);
        return result;
    }

    private static unsafe byte[] ToByteArray(Surface surface, ColorType colorType = ColorType.Bgra8888, AlphaType alphaType = AlphaType.Premul)
    {
        int width = surface.Size.X;
        int height = surface.Size.Y;
        var imageInfo = new ImageInfo(width, height, colorType, alphaType, ColorSpace.CreateSrgb());

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
