using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.Tests;

public class FramebufferExtensionTests
{
    [Fact]
    public void TestThatExtractPixelsFromBitmapReturnsCorrectAmountOfPixels()
    {
        var bitmap = new WriteableBitmap(new PixelSize(10, 10), new Vector(96, 96), PixelFormats.Bgra8888);
        var pixels = bitmap.ExtractPixels();
        Assert.Equal(400, pixels.Length);
    }

    [Theory]
    [InlineData(255, 0, 0, 255)]
    [InlineData(0, 255, 0, 255)]
    [InlineData(0, 0, 255, 255)]
    [InlineData(255, 255, 255, 255)]
    [InlineData(0, 0, 0, 255)]
    [InlineData(255, 255, 255, 0)]
    [InlineData(0, 0, 0, 0)]
    public void TestThatWritePixelSetsCorrectColor(byte r, byte g, byte b, byte a)
    {
        var bitmap = new WriteableBitmap(new PixelSize(1, 1), new Vector(96, 96), PixelFormats.Bgra8888);
        using var framebuffer = bitmap.Lock();
        framebuffer.WritePixel(0, 0, Color.FromArgb(a, r, g, b));
        var pixels = framebuffer.GetPixels();
        Assert.Equal(r, pixels[2]);
        Assert.Equal(g, pixels[1]);
        Assert.Equal(b, pixels[0]);
        Assert.Equal(a, pixels[3]);
    }

    [Theory]
    [InlineData(255, 0, 0, 255)]
    [InlineData(0, 255, 0, 255)]
    [InlineData(0, 0, 255, 255)]
    [InlineData(255, 255, 255, 255)]
    [InlineData(0, 0, 0, 255)]
    [InlineData(255, 255, 255, 0)]
    [InlineData(0, 0, 0, 0)]
    public void TestThatWritePixelsSetsCorrectColor(byte r, byte g, byte b, byte a)
    {
        var bitmap = new WriteableBitmap(new PixelSize(1, 1), new Vector(96, 96), PixelFormats.Bgra8888);
        using var framebuffer = bitmap.Lock();
        framebuffer.WritePixels(new RectI(0, 0, 1, 1), new byte[] { b, g, r, a });
        var pixels = framebuffer.GetPixels();
        Assert.Equal(r, pixels[2]);
        Assert.Equal(g, pixels[1]);
        Assert.Equal(b, pixels[0]);
        Assert.Equal(a, pixels[3]);
    }

    [Theory]
    [InlineData(255, 0, 0, 255)]
    [InlineData(0, 255, 0, 255)]
    [InlineData(0, 0, 255, 255)]
    [InlineData(255, 255, 255, 255)]
    [InlineData(0, 0, 0, 255)]
    [InlineData(255, 255, 255, 0)]
    [InlineData(0, 0, 0, 0)]
    public void TestThatGetPixelsReturnsCorrectColor(byte r, byte g, byte b, byte a)
    {
        WriteableBitmap bitmap = new WriteableBitmap(new PixelSize(1, 1), new Vector(96, 96), PixelFormats.Bgra8888, AlphaFormat.Premul);
        using var framebuffer = bitmap.Lock();
        framebuffer.WritePixel(0, 0, Color.FromArgb(a, r, g, b));
        var color = framebuffer.GetPixel(0, 0);
        Assert.Equal(r, color.R);
        Assert.Equal(g, color.G);
        Assert.Equal(b, color.B);
        Assert.Equal(a, color.A);
    }

    [Fact]
    public void TestThatExtractPixelsFromBitmapReturnsCorrectBgra8888ByteSequence()
    {
        var bitmap = new WriteableBitmap(new PixelSize(4, 1), new Vector(96, 96), PixelFormats.Bgra8888);
        using var framebuffer = bitmap.Lock();
        framebuffer.WritePixel(0, 0, Color.FromArgb(255, 255, 0, 0)); //Red
        framebuffer.WritePixel(1, 0, Color.FromArgb(255, 0, 255, 0)); //Green
        framebuffer.WritePixel(2, 0, Color.FromArgb(255, 0, 0, 255)); //Blue
        framebuffer.WritePixel(3, 0, Color.FromArgb(255, 255, 255, 255)); //White
        var pixels = bitmap.ExtractPixels();

        Assert.Equal(0, pixels[0]);
        Assert.Equal(0, pixels[1]);
        Assert.Equal(255, pixels[2]);
        Assert.Equal(255, pixels[3]);

        Assert.Equal(0, pixels[4]);
        Assert.Equal(255, pixels[5]);
        Assert.Equal(0, pixels[6]);
        Assert.Equal(255, pixels[7]);

        Assert.Equal(255, pixels[8]);
        Assert.Equal(0, pixels[9]);
        Assert.Equal(0, pixels[10]);
        Assert.Equal(255, pixels[11]);

        Assert.Equal(255, pixels[12]);
        Assert.Equal(255, pixels[13]);
        Assert.Equal(255, pixels[14]);
        Assert.Equal(255, pixels[15]);
    }
}
