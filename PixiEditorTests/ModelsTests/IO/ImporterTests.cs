using PixiEditor.Exceptions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.IO;
using PixiEditorTests.ModelsTests.ColorsTests;
using SkiaSharp;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace PixiEditorTests.ModelsTests.IO
{
    public class ImporterTests
    {
        private readonly string testImagePath;
        private readonly string testCorruptedPixiImagePath;

        // I am not testing ImportDocument, because it's just a wrapper for BinarySerialization which is tested.
        public ImporterTests()
        {
            testImagePath = $"{Environment.CurrentDirectory}\\..\\..\\..\\ModelsTests\\IO\\TestImage.png";
            testCorruptedPixiImagePath = $"{Environment.CurrentDirectory}\\..\\..\\..\\ModelsTests\\IO\\CorruptedPixiFile.pixi";
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
            SKColor color = new SKColor(255, 255, 0, 0);
            Surface image = Importer.ImportSurface(testImagePath);

            Assert.NotNull(image);
            Assert.Equal(5, image.Width);
            Assert.Equal(5, image.Height);
            Assert.Equal(color, image.GetSRGBPixel(0, 0)); // Top left
            Assert.Equal(color, image.GetSRGBPixel(4, 4)); // Bottom right
            Assert.Equal(color, image.GetSRGBPixel(0, 4)); // Bottom left
            Assert.Equal(color, image.GetSRGBPixel(4, 0)); // Top right
            Assert.Equal(color, image.GetSRGBPixel(2, 2)); // Middle center
        }

        [Fact]
        public void TestThatImporterThrowsCorruptedFileExceptionOnWrongPixiFileWithSupportedExtension()
        {
            Assert.Throws<CorruptedFileException>(() => { Importer.ImportDocument(testCorruptedPixiImagePath); });
        }

        [Theory]
        [InlineData("CorruptedPNG.png")]
        [InlineData("CorruptedPNG2.png")]
        [InlineData("CorruptedJpg.jpg")]
        public void TestThatImporterThrowsCorruptedFileExceptionOnWrongImageFileWithSupportedExtension(string fileName)
        {
            string imagePath = $"{Environment.CurrentDirectory}\\..\\..\\..\\ModelsTests\\IO\\{fileName}";
            Assert.Throws<CorruptedFileException>(() => { Importer.ImportSurface(imagePath); });
        }

        [Fact]
        public void TestThatImportImageResizes()
        {
            Surface image = Importer.ImportImage(testImagePath, 10, 10);

            Assert.Equal(10, image.Width);
            Assert.Equal(10, image.Height);
        }

        [Fact]
        public void TestSaveAndLoadGZippedBytes()
        {
            using Surface original = new Surface(123, 456);
            original.SkiaSurface.Canvas.Clear(ExtendedColorTests.red);
            using SKPaint paint = new SKPaint();
            paint.BlendMode = SKBlendMode.Src;
            paint.Color = new SKColor(128, 64, 32, 16);
            original.SkiaSurface.Canvas.DrawRect(10, 10, 20, 20, paint);
            Exporter.SaveAsGZippedBytes("pleasedontoverwritethings", original);
            using var loaded = Importer.LoadFromGZippedBytes("pleasedontoverwritethings");
            File.Delete("pleasedontoverwritethings");
            Assert.Equal(original.Width, loaded.Width);
            Assert.Equal(original.Height, loaded.Height);
            Assert.Equal(original.GetSRGBPixel(0, 0), loaded.GetSRGBPixel(0, 0));
            Assert.Equal(original.GetSRGBPixel(15, 15), loaded.GetSRGBPixel(15, 15));
        }
    }
}
