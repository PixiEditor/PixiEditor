using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PixiEditor.Avalonia.Helpers.Extensions;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.Avalonia.Helpers;

internal static class WriteableBitmapHelpers
{
    public static WriteableBitmap FromPbgra8888Array(byte[] pbgra8888, VecI size)
    {
        WriteableBitmap bitmap = new WriteableBitmap(new PixelSize(size.X, size.Y), new Vector(96, 96), PixelFormats.Bgra8888, AlphaFormat.Premul);
        using var frameBuffer = bitmap.Lock();
        frameBuffer.WritePixels(new RectI(0, 0, size.X, size.Y), pbgra8888);
        return bitmap;
    }

    public static WriteableBitmap CreateBitmap(VecI size)
    {
        return new WriteableBitmap(new PixelSize(Math.Max(size.X, 1), Math.Max(size.Y, 1)), new Vector(96, 96), PixelFormats.Bgra8888, AlphaFormat.Premul);
    }

    public static DrawingSurface CreateDrawingSurface(WriteableBitmap bitmap)
    {
        using var frameBuffer = bitmap.Lock();
        return DrawingSurface.Create(
            new ImageInfo(bitmap.PixelSize.Width, bitmap.PixelSize.Height, ColorType.Bgra8888, AlphaType.Premul,
                ColorSpace.CreateSrgb()),
            frameBuffer.Address,
            frameBuffer.RowBytes);
    }
}
