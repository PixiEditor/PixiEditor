using Avalonia.Media;
using Avalonia.Platform;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Helpers.Extensions;

public static class LockedFramebufferExtensions
{
    public static Span<byte> GetPixels(this ILockedFramebuffer framebuffer)
    {
        unsafe
        {
            return new Span<byte>((byte*)framebuffer.Address, framebuffer.RowBytes * framebuffer.Size.Height);
        }
    }

    public static Color GetPixel(this ILockedFramebuffer framebuffer, int x, int y)
    {
        unsafe
        {
            if(framebuffer.Format != PixelFormat.Bgra8888)
                throw new ArgumentException("Only Bgra8888 is supported");

            var bytesPerPixel = framebuffer.Format.BitsPerPixel / 8; //TODO: check if bits per pixel is correct
            var zero = (byte*)framebuffer.Address;
            var offset = framebuffer.RowBytes * y + bytesPerPixel * x;
            byte a = zero[offset + 3];
            byte r = zero[offset + 2];
            byte g = zero[offset + 1];
            byte b = zero[offset];

            return Color.FromArgb(a, r, g, b);
        }
    }

    public static void WritePixels(this ILockedFramebuffer framebuffer, RectI rectI, byte[] pixelBytes)
    {
        //TODO: Idk if this is correct
        Span<byte> pixels = framebuffer.GetPixels();
        int rowBytes = framebuffer.RowBytes;
        int width = framebuffer.Size.Width;

        int startX = Math.Max(0, rectI.X);
        int endX = Math.Min(width, rectI.X + rectI.Width);

        int startY = Math.Max(0, rectI.Y);
        int endY = Math.Min(framebuffer.Size.Height, rectI.Y + rectI.Height);

        int bytePerPixel = framebuffer.Format.BitsPerPixel / 8;

        for (int y = startY; y < endY; y++)
        {
            int rowIndex = y * rowBytes;
            int startOffset = rowIndex + startX * bytePerPixel;
            int endOffset = rowIndex + endX * bytePerPixel;

            int srcRowStartIndex = (y - rectI.Y) * rectI.Width * bytePerPixel;

            pixelBytes.AsSpan(srcRowStartIndex, endOffset - startOffset).CopyTo(pixels.Slice(startOffset));
        }
    }

    public static void WritePixel(this ILockedFramebuffer framebuffer, int x, int y, Color color)
    {
        unsafe
        {
            var bytesPerPixel = framebuffer.Format.BitsPerPixel / 8;
            var zero = (byte*)framebuffer.Address;
            var offset = framebuffer.RowBytes * y + bytesPerPixel * x;
            zero[offset + 3] = color.A;
            zero[offset + 2] = color.R;
            zero[offset + 1] = color.G;
            zero[offset] = color.B;
        }
    }
}
