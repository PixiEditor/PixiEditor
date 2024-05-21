using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.Tests;

public class WriteableBitmapUtilityTests
{
    [Fact]
    public void TestThatFromBgra8888ArrayReturnsCorrectWriteableBitmap()
    {
        byte[] bgra8888 = new byte[4];
        Color color = Color.FromArgb(5, 150, 200, 255);
        bgra8888[0] = color.B;
        bgra8888[1] = color.G;
        bgra8888[2] = color.R;
        bgra8888[3] = color.A;

        VecI size = new(1, 1);
        WriteableBitmap result = WriteableBitmapUtility.FromBgra8888Array(bgra8888, size);
        Assert.NotNull(result);
        Assert.Equal(1, result.PixelSize.Width);
        Assert.Equal(1, result.PixelSize.Height);
        Assert.Equal(96, result.Dpi.X);
        Assert.Equal(96, result.Dpi.Y);
        Assert.Equal(PixelFormats.Bgra8888, result.Format);

        using ILockedFramebuffer frameBuffer = result.Lock();
        Assert.Equal(4, frameBuffer.RowBytes);
        Assert.Equal(1, frameBuffer.Size.Width);
        Assert.Equal(1, frameBuffer.Size.Height);
        Assert.Equal(color, frameBuffer.GetPixel(0, 0));
    }
}
