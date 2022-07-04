using PixiEditor.Models.DataHolders;
using SkiaSharp;
using Xunit;

namespace PixiEditorTests.ModelsTests.DataHoldersTests;
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
public class SurfaceTests
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
{
    SKColor redColor = new SKColor(254, 2, 3);
    SKColor greenColor = new SKColor(6, 224, 3);
    SKPaint redPaint;
    SKPaint greenPaint;

    public SurfaceTests()
    {
        redPaint = new SKPaint()
        {
            Color = redColor,
        };
        greenPaint = new SKPaint()
        {
            Color = greenColor,
        };
    }

    [Fact]
    public void TestSurfaceSRGBPixelManipulation()
    {
        using Surface surface = new Surface(128, 200);
        surface.SkiaSurface.Canvas.Clear(SKColors.Red);
        surface.SkiaSurface.Canvas.DrawRect(new SKRect(10, 10, 70, 70), redPaint);
        surface.SetSRGBPixel(73, 21, greenColor);
        Assert.Equal(redColor, surface.GetSRGBPixel(14, 14));
        Assert.Equal(greenColor, surface.GetSRGBPixel(73, 21));
    }

    [Fact]
    public void TestSurfacePbgraBytes()
    {
        byte[] bytes = new byte[]
        {
            123, 121, 141, 255,  014, 010, 007, 255,
            042, 022, 055, 128,  024, 020, 021, 128,
            040, 010, 055, 064,  042, 022, 005, 064,
            005, 009, 001, 032,  001, 011, 016, 032,
        };
        using Surface surface = new Surface(2, 4, bytes);
        Assert.Equal(new SKColor(141, 121, 123, 255), surface.GetSRGBPixel(0, 0));
        Assert.Equal(new SKColor(110, 44, 84, 128), surface.GetSRGBPixel(0, 1));
        var newBytes = surface.ToByteArray();
        Assert.Equal(bytes, newBytes);
    }

    [Fact]
    public void TestCloneSurface()
    {
        using Surface original = new Surface(30, 40);
        original.SkiaSurface.Canvas.Clear(redColor);
        original.SkiaSurface.Canvas.DrawRect(5, 5, 10, 10, greenPaint);
        using Surface clone = new Surface(original);
        Assert.NotSame(original.SkiaSurface, clone.SkiaSurface);
        Assert.NotSame(original.SkiaSurface.Canvas, clone.SkiaSurface.Canvas);
        Assert.Equal(redColor, clone.GetSRGBPixel(3, 3));
        Assert.Equal(greenColor, clone.GetSRGBPixel(6, 6));
    }

    [Fact]
    public void TestSurfaceNearestNeighborResize()
    {
        using Surface original = new Surface(30, 40);
        original.SkiaSurface.Canvas.Clear(redColor);
        original.SkiaSurface.Canvas.DrawRect(5, 5, 20, 20, greenPaint);
        using Surface resized = original.ResizeNearestNeighbor(10, 10);
        Assert.Equal(10, resized.Width);
        Assert.Equal(10, resized.Height);
        Assert.Equal(redColor, resized.GetSRGBPixel(0, 0));
        Assert.Equal(redColor, resized.GetSRGBPixel(9, 9));
        Assert.Equal(greenColor, resized.GetSRGBPixel(5, 5));
    }

    [Fact]
    public void TestSurfaceToWriteableBitmap()
    {
        using Surface original = new Surface(30, 40);
        original.SkiaSurface.Canvas.Clear(redColor);
        original.SkiaSurface.Canvas.DrawRect(5, 5, 20, 20, greenPaint);
        var bitmap = original.ToWriteableBitmap();
        byte[] pixels = new byte[30 * 40 * 4];
        bitmap.CopyPixels(pixels, 30 * 4, 0);
        Assert.Equal(redColor, new SKColor(pixels[2], pixels[1], pixels[0], pixels[3]));
        int offset = (30 * 5 + 5) * 4;
        Assert.Equal(greenColor, new SKColor(pixels[2 + offset], pixels[1 + offset], pixels[0 + offset], pixels[3 + offset]));
    }

    [Fact]
    public void TestSurfaceFromWriteableBitmap()
    {
        using Surface original = new Surface(30, 30);
        original.SkiaSurface.Canvas.Clear(SKColors.Transparent);
        original.SkiaSurface.Canvas.DrawRect(5, 5, 20, 20, redPaint);
        original.SkiaSurface.Canvas.DrawRect(10, 10, 20, 20, greenPaint);
        using Surface fromWriteable = new Surface(original.ToWriteableBitmap());
        Assert.Equal(original.GetSRGBPixel(0, 0), fromWriteable.GetSRGBPixel(0, 0));
        Assert.Equal(original.GetSRGBPixel(6, 6), fromWriteable.GetSRGBPixel(6, 6));
        Assert.Equal(original.GetSRGBPixel(15, 15), fromWriteable.GetSRGBPixel(15, 15));
    }
}