using Avalonia.Media.Imaging;
using ChunkyImageLib;
using PixiEditor.Helpers.Extensions;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Interop.Avalonia.Core;
using Drawie.Numerics;

namespace PixiEditor.Helpers;

public static class SurfaceHelpers
{
    public static Surface FromBitmap(Bitmap original)
    {
        if(original.Format == null) throw new ArgumentException("Bitmap format must be non-null");

        ColorType color = original.Format.Value.ToColorType(out AlphaType alpha);
        if (original.PixelSize.Width <= 0 || original.PixelSize.Height <= 0)
            throw new ArgumentException("Surface dimensions must be non-zero");

        int stride = (original.PixelSize.Width * original.Format.Value.BitsPerPixel + 7) / 8;
        byte[] pixels = original.ExtractPixels();

        Surface surface = new Surface(new VecI(original.PixelSize.Width, original.PixelSize.Height));
        surface.DrawBytes(surface.Size, pixels, color, alpha);
        return surface;
    }

    public static WriteableBitmap ToWriteableBitmap(this Surface surface)
    {
        WriteableBitmap result = WriteableBitmapUtility.CreateBitmap(surface.Size);
        using var framebuffer = result.Lock();
        var dirty = new RectI(0, 0, surface.Size.X, surface.Size.Y);
        framebuffer.WritePixels(dirty, ToByteArray(surface));
        //result.AddDirtyRect(dirty); //TODO: Look at this later, no DirtyRect in Avalonia
        return result;
    }

    public static unsafe byte[] ToByteArray(this Surface surface, ColorType colorType = ColorType.Bgra8888, AlphaType alphaType = AlphaType.Premul, ColorSpace colorSpace = null)
    {
        using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
        int width = surface.Size.X;
        int height = surface.Size.Y;
        var imageInfo = new ImageInfo(width, height, colorType, alphaType, colorSpace == null ? surface.ImageInfo.ColorSpace : colorSpace);

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
