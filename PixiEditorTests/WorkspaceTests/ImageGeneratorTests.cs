using NUnit.Framework;
using PixiEditor.Models.Images;
using System.Windows.Controls;
using System.Windows.Media;

namespace PixiEditorTests.ToolsTests
{
    [TestFixture, Apartment(System.Threading.ApartmentState.STA)]
    public class ImageGeneratorTests
    {
        [TestCase(16, 16)]
        [TestCase(1024, 12)]
        [TestCase(50000, 50000)]
        public void ImageIsPixelArtReady(int width, int height)
        {
            Image img = ImageGenerator.GenerateForPixelArts(width, height);

            Assert.IsTrue(img.Stretch == Stretch.Uniform && RenderOptions.GetBitmapScalingMode(img) == BitmapScalingMode.NearestNeighbor
                && RenderOptions.GetEdgeMode(img) == EdgeMode.Aliased && img.Width == width && img.Height == height);
        }
    }
}
