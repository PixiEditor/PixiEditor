using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.Avalonia.Helpers;

public static class StructureHelpers
{
    public const int PreviewSize = 48;
    /// <summary>
    /// Calculates the size of a scaled-down preview for a given size of layer tight bounds.
    /// </summary>
    public static VecI CalculatePreviewSize(VecI tightBoundsSize)
    {
        double proportions = tightBoundsSize.Y / (double)tightBoundsSize.X;
        const int prSize = PreviewSize;
        return proportions > 1 ?
            new VecI(Math.Max((int)Math.Round(prSize / proportions), 1), prSize) :
            new VecI(prSize, Math.Max((int)Math.Round(prSize * proportions), 1));
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
