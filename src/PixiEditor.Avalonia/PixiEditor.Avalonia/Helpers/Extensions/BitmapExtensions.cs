using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;

namespace PixiEditor.Avalonia.Helpers.Extensions;

public static class BitmapExtensions
{
    public static byte[] ExtractPixels(this Bitmap source)
    {
        var size = source.PixelSize;
        var stride = size.Width * 4;
        int bufferSize = stride * size.Height;

        byte[] target = new byte[bufferSize];

        IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(target, 0);

        source.CopyPixels(new PixelRect(0, 0, size.Width, size.Height), ptr, bufferSize, stride);

        return target;
    }
}
