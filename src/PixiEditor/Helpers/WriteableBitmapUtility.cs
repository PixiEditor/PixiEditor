using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PixiEditor.Helpers.Extensions;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace PixiEditor.Helpers;

public static class WriteableBitmapUtility
{
    public static WriteableBitmap FromBgra8888Array(byte[] bgra8888, VecI size)
    {
        WriteableBitmap bitmap = new WriteableBitmap(new PixelSize(size.X, size.Y), new Vector(96, 96), PixelFormats.Bgra8888, AlphaFormat.Premul);
        using var frameBuffer = bitmap.Lock();
        frameBuffer.WritePixels(new RectI(0, 0, size.X, size.Y), bgra8888);
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
