using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChunkyImageLib;
using SkiaSharp;

namespace PixiEditorPrototype.Helpers;

public static class SurfaceHelpers
{
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
            if (!surface.SkiaSurface.ReadPixels(imageInfo, new IntPtr(pointer), imageInfo.RowBytes, 0, 0))
            {
                throw new InvalidOperationException("Could not read surface into buffer");
            }
        }

        return buffer;
    }
}
