using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using SkiaSharp;
using Xunit;

namespace PixiEditorTests.ModelsTests.LayersTests
{
    public class LayerTests
    {
        //[Fact]
        //public void TestThatEmptyLayerGeneratesCorrectly()
        //{
        //    Layer layer = new Layer("layer");

        //    Assert.Equal("layer", layer.Name);
        //    Assert.Equal(1, layer.Width);
        //    Assert.Equal(1, layer.Height);
        //    Assert.Equal(1, layer.LayerBitmap.Width);
        //    Assert.Equal(1, layer.LayerBitmap.Height);
        //}

        //[Fact]
        //public void TestThatEmptyLayerWithSizeGeneratesCorrectly()
        //{
        //    Layer layer = new Layer("layer", 10, 10);

        //    Assert.Equal("layer", layer.Name);
        //    Assert.Equal(10, layer.Width);
        //    Assert.Equal(10, layer.Height);
        //    Assert.Equal(10, layer.LayerBitmap.Width);
        //    Assert.Equal(10, layer.LayerBitmap.Height);
        //}

        //[Fact]
        //public void TestThatLayerFromBitmapGeneratesCorrectly()
        //{
        //    using Surface bmp = new Surface(10, 10);

        //    Layer layer = new Layer("layer", bmp);

        //    Assert.Equal("layer", layer.Name);
        //    Assert.Equal(10, layer.Width);
        //    Assert.Equal(10, layer.Height);
        //    Assert.Equal(10, layer.LayerBitmap.Width);
        //    Assert.Equal(10, layer.LayerBitmap.Height);
        //}

        //[Fact]
        //public void TestThatCloneClonesCorrectly()
        //{
        //    Layer layer = new Layer("test", 5, 2);

        //    Layer clone = layer.Clone();

        //    LayersTestHelper.LayersAreEqual(layer, clone);
        //}

        //[Fact]
        //public void TestThatCloneIsMakingDeepCopyOfBitmap()
        //{
        //    Layer layer = new Layer("test", 5, 2);

        //    Layer clone = layer.Clone();

        //    clone.LayerBitmap.SetSRGBPixel(0, 0, SKColors.Lime); // Actually we are checking if modifying clone bitmap does not affect original

        //    Assert.NotEqual(SKColors.Lime, layer.GetPixel(0, 0));
        //}

        //[Fact]
        //public void TestThatResizeResizesBitmap()
        //{
        //    Layer layer = new Layer("layer", 1, 1);

        //    layer.SetPixel(new Coordinates(0, 0), SKColors.Black);

        //    layer.Resize(2, 2, 2, 2);

        //    Assert.Equal(2, layer.Width);
        //    Assert.Equal(2, layer.Height);
        //    Assert.Equal(2, layer.MaxWidth);
        //    Assert.Equal(2, layer.MaxHeight);

        //    // 4 is new area of bitmap
        //    for (int y = 0; y < layer.Height; y++)
        //    {
        //        for (int x = 0; x < layer.Width; x++)
        //        {
        //            Assert.Equal(SKColors.Black, layer.GetPixel(x, y));
        //        }
        //    }
        //}

        //[Fact]
        //public void TestThatGetPixelReturnsTransparentIfOutOfBounds()
        //{
        //    Layer layer = new Layer("layer");

        //    Assert.Equal(0, layer.GetPixel(-1, 999).Alpha);
        //}

        //[Fact]
        //public void TestThatSetPixelsSetsPixels() // This also tests if Dynamic Resize works
        //{
        //    Coordinates[] pixels = { new Coordinates(4, 2), new Coordinates(0, 0), new Coordinates(15, 2) };

        //    Layer layer = new Layer("layer");

        //    layer.SetPixels(BitmapPixelChanges.FromSingleColoredArray(pixels, SKColors.Lime));

        //    for (int i = 0; i < pixels.Length; i++)
        //    {
        //        Assert.Equal(SKColors.Lime, layer.GetPixelWithOffset(pixels[i].X, pixels[i].Y));
        //    }
        //}

        //[Fact]
        //public void TestThatClipCanvasResizesBitmapCorrectly()
        //{
        //    Layer layer = new Layer("layer", 10, 10);
        //    layer.SetPixel(new Coordinates(4, 4), SKColors.Blue);

        //    layer.ClipCanvas();

        //    Assert.Equal(1, layer.Width);
        //    Assert.Equal(1, layer.Height);
        //    Assert.Equal(SKColors.Blue, layer.GetPixel(0, 0));
        //}
    }
}
