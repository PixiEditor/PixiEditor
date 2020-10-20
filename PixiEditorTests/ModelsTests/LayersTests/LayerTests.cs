using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using Xunit;

namespace PixiEditorTests.ModelsTests.LayersTests
{
    public class LayerTests
    {
        [Fact]
        public void TestThatEmptyLayerGeneratesCorrectly()
        {
            var layer = new Layer("layer");

            Assert.Equal("layer", layer.Name);
            Assert.Equal(0, layer.Width);
            Assert.Equal(0, layer.Height);
            Assert.Equal(1, layer.LayerBitmap.PixelWidth);
            Assert.Equal(1, layer.LayerBitmap.PixelHeight);
        }

        [Fact]
        public void TestThatEmptyLayerWithSizeGeneratesCorrectly()
        {
            var layer = new Layer("layer", 10, 10);

            Assert.Equal("layer", layer.Name);
            Assert.Equal(10, layer.Width);
            Assert.Equal(10, layer.Height);
            Assert.Equal(10, layer.LayerBitmap.PixelWidth);
            Assert.Equal(10, layer.LayerBitmap.PixelHeight);
        }

        [Fact]
        public void TestThatLayerFromBitmapGeneratesCorrectly()
        {
            var bmp = BitmapFactory.New(10, 10);

            var layer = new Layer("layer", bmp);

            Assert.Equal("layer", layer.Name);
            Assert.Equal(10, layer.Width);
            Assert.Equal(10, layer.Height);
            Assert.Equal(10, layer.LayerBitmap.PixelWidth);
            Assert.Equal(10, layer.LayerBitmap.PixelHeight);
        }

        [Fact]
        public void TestThatCloneClonesCorrectly()
        {
            var layer = new Layer("test", 5, 2);

            var clone = layer.Clone();

            Assert.Equal(layer.Name, clone.Name);
            Assert.Equal(layer.Offset, clone.Offset);
            Assert.Equal(layer.Width, clone.Width);
            Assert.Equal(layer.Height, clone.Height);
            Assert.Equal(layer.MaxHeight, clone.MaxHeight);
            Assert.Equal(layer.MaxWidth, clone.MaxWidth);
            Assert.Equal(layer.Opacity, clone.Opacity);
            Assert.Equal(layer.IsVisible, clone.IsVisible);
            Assert.Equal(layer.IsRenaming, clone.IsRenaming);
            Assert.Equal(layer.ConvertBitmapToBytes(), clone.ConvertBitmapToBytes());
        }

        [Fact]
        public void TestThatCloneIsMakingDeepCopyOfBitmap()
        {
            var layer = new Layer("test", 5, 2);

            var clone = layer.Clone();

            clone.LayerBitmap.SetPixel(0, 0, Colors.Green); //Actually we are checking if modifying clone bitmap does not affect original

            Assert.NotEqual(Colors.Green, layer.GetPixel(0, 0));
        }

        [Fact]
        public void TestThatResizeResizesBitmap()
        {
            var layer = new Layer("layer", 1, 1);

            layer.SetPixel(new Coordinates(0, 0), Colors.Black);

            layer.Resize(2, 2, 2, 2);

            Assert.Equal(2, layer.Width);
            Assert.Equal(2, layer.Height);
            Assert.Equal(2, layer.MaxWidth);
            Assert.Equal(2, layer.MaxHeight);

            for (var y = 0; y < layer.Height; y++) //4 is new area of bitmap
            for (var x = 0; x < layer.Width; x++)
                Assert.Equal(Colors.Black, layer.GetPixel(x, y));
        }

        [Fact]
        public void TestThatGetPixelReturnsTransparentIfOutOfBounds()
        {
            var layer = new Layer("layer");

            Assert.Equal(0, layer.GetPixel(-1, 999).A);
        }

        [Fact]
        public void TestThatSetPixelsSetsPixels() //This also tests if Dynamic Resize works
        {
            Coordinates[] pixels = {new Coordinates(4, 2), new Coordinates(0, 0), new Coordinates(15, 2)};

            var layer = new Layer("layer");

            layer.SetPixels(BitmapPixelChanges.FromSingleColoredArray(pixels, Colors.Green));

            for (var i = 0; i < pixels.Length; i++) Assert.Equal(Colors.Green, layer.GetPixelWithOffset(pixels[i].X, pixels[i].Y));
        }

        [Fact]
        public void TestThatClipCanvasResizesBitmapCorrectly()
        {
            var layer = new Layer("layer", 10, 10);
            layer.SetPixel(new Coordinates(4, 4), Colors.Blue);

            layer.ClipCanvas();

            Assert.Equal(1, layer.Width);
            Assert.Equal(1, layer.Height);
            Assert.Equal(Colors.Blue, layer.GetPixel(0, 0));
        }
    }
}