using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Models.IO;
using Xunit;

namespace PixiEditorTests.ModelsTests.IO
{
    public class ImporterTests
    {
        private readonly string testImagePath;

        // I am not testing ImportDocument, because it's just a wrapper for BinarySerialization which is tested.
        public ImporterTests()
        {
            testImagePath = $"{Environment.CurrentDirectory}\\..\\..\\..\\ModelsTests\\IO\\TestImage.png";
        }

        [Theory]
        [InlineData("wubba.png")]
        [InlineData("lubba.pixi")]
        [InlineData("dub.jpeg")]
        [InlineData("-.JPEG")]
        [InlineData("dub.jpg")]
        public void TestThatIsSupportedFile(string file)
        {
            Assert.True(Importer.IsSupportedFile(file));
        }

        [Fact]
        public void TestThatImportImageImportsImage()
        {
            Color color = Color.FromArgb(255, 255, 0, 0);
            WriteableBitmap image = Importer.ImportImage(testImagePath);

            Assert.NotNull(image);
            Assert.Equal(5, image.PixelWidth);
            Assert.Equal(5, image.PixelHeight);
            Assert.Equal(color, image.GetPixel(0, 0)); // Top left
            Assert.Equal(color, image.GetPixel(4, 4)); // Bottom right
            Assert.Equal(color, image.GetPixel(0, 4)); // Bottom left
            Assert.Equal(color, image.GetPixel(4, 0)); // Top right
            Assert.Equal(color, image.GetPixel(2, 2)); // Middle center
        }

        [Fact]
        public void TestThatImportImageResizes()
        {
            WriteableBitmap image = Importer.ImportImage(testImagePath, 10, 10);

            Assert.Equal(10, image.PixelWidth);
            Assert.Equal(10, image.PixelHeight);
        }
    }
}