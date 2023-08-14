using Avalonia.Media;
using Avalonia.Platform;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Helpers.Extensions;

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
            var bytesPerPixel = framebuffer.Format.BitsPerPixel / 8; //TODO: check if bits per pixel is correct
            var zero = (byte*)framebuffer.Address;
            var offset = framebuffer.RowBytes * y + bytesPerPixel * x;
            return Color.FromArgb(255, zero[offset + 2], zero[offset + 1], zero[offset]);
        }
    }

    public static void WritePixels(this ILockedFramebuffer framebuffer, RectI rectI, byte[] pbgra8888Bytes)
    {
        //TODO: Idk if this is correct
        Span<byte> pixels = framebuffer.GetPixels();
        int rowBytes = framebuffer.RowBytes;
        int width = framebuffer.Size.Width;

        int startX = Math.Max(0, rectI.X);
        int endX = Math.Min(width, rectI.X + rectI.Width);

        int startY = Math.Max(0, rectI.Y);
        int endY = Math.Min(framebuffer.Size.Height, rectI.Y + rectI.Height);

        int bytePerPixel = 4; // BGRA8888 has 4 bytes per pixel

        for (int y = startY; y < endY; y++)
        {
            int rowIndex = y * rowBytes;
            int startOffset = rowIndex + startX * bytePerPixel;
            int endOffset = rowIndex + endX * bytePerPixel;

            int srcRowStartIndex = (y - rectI.Y) * rectI.Width * bytePerPixel;

            pbgra8888Bytes.AsSpan(srcRowStartIndex, endOffset - startOffset).CopyTo(pixels.Slice(startOffset));
        }
    }

    public static void WritePixel(this ILockedFramebuffer framebuffer, int x, int y, Color color)
    {
        unsafe
        {
            var bytesPerPixel = framebuffer.Format.BitsPerPixel / 8; //TODO: check if bits per pixel is correct
            var zero = (byte*)framebuffer.Address;
            var offset = framebuffer.RowBytes * y + bytesPerPixel * x;
            zero[offset + 3] = color.A;
            zero[offset + 2] = color.R;
            zero[offset + 1] = color.G;
            zero[offset] = color.B;
        }
    }
}
