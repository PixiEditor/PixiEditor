using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.DrawingApi.Core.Numerics;
using SkiaSharp;

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

    /*[Fact]
    public void TestThatExtractPixelsFromBitmapReturnsCorrectBgra8888ByteSequence()
    {
        var bitmap = new WriteableBitmap(new PixelSize(4, 1), new Vector(96, 96), PixelFormats.Bgra8888);
        using var framebuffer = bitmap.Lock();
        framebuffer.WritePixel();
        var pixels = bitmap.ExtractPixels();

        Assert.Equal(4, pixels.Length);
        Assert.Equal(0, pixels[0]);
        Assert.Equal(0, pixels[1]);
    }*/
}
