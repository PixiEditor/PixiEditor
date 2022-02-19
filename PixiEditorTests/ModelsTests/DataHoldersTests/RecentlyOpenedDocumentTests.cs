using PixiEditor.Models.DataHolders;
using PixiEditor.Parser;
using System;
using Xunit;

namespace PixiEditorTests.ModelsTests.DataHoldersTests
{
    [Collection("Application collection")]
    public class RecentlyOpenedDocumentTests
    {
        [Fact]
        public void TestThatForBigPixiFilesPreviewImageIsResizedToMaxSize()
        {
            string bigFilePath = $@"{Environment.CurrentDirectory}\..\..\..\ModelsTests\IO\BigPixiFile.pixi";
            RecentlyOpenedDocument recentlyOpenedDocument = new RecentlyOpenedDocument(bigFilePath);

            var bigPixiFilePreviewImage = recentlyOpenedDocument.PreviewBitmap;

            const int MaxWidthInPixels = 1080;
            Assert.True(bigPixiFilePreviewImage.PixelWidth <= MaxWidthInPixels);

            const int MaxHeightInPixels = 1080;
            Assert.True(bigPixiFilePreviewImage.PixelHeight <= MaxHeightInPixels);

            // This is a workaround for checking the Pixi file layers.
            Assert.True(PixiParser.Deserialize(bigFilePath).Layers.Count <= 5);
        }

        [Fact]
        public void TestThatForSmallEnoughPixiFilesPreviewImageIsLoaded()
        {
            string smallEnoughFilePath = $@"{Environment.CurrentDirectory}\..\..\..\ModelsTests\IO\SmallEnoughPixiFile.pixi";
            RecentlyOpenedDocument recentlyOpenedDocument = new RecentlyOpenedDocument(smallEnoughFilePath);

            var smallEnoughFilePreviewImage = recentlyOpenedDocument.PreviewBitmap;

            Assert.NotNull(smallEnoughFilePreviewImage);
        }

        [Theory]
        [InlineData("png")]
        [InlineData("jpg")]
        [InlineData("jpeg")]
        public void TestThatForBigImageFilesPreviewImageIsResizedToMaxSize(string imageFormat)
        {
            string bigImageFilePath = $@"{Environment.CurrentDirectory}\..\..\..\ModelsTests\IO\BigImage.{imageFormat}";
            RecentlyOpenedDocument recentlyOpenedDocument = new RecentlyOpenedDocument(bigImageFilePath);

            var bigImagePreviewImage = recentlyOpenedDocument.PreviewBitmap;

            const int MaxWidthInPixels = 2048;
            Assert.True(bigImagePreviewImage.PixelWidth <= MaxWidthInPixels);

            const int MaxHeightInPixels = 2048;
            Assert.True(bigImagePreviewImage.PixelHeight <= MaxHeightInPixels);
        }

        [Theory]
        [InlineData("png")]
        [InlineData("jpg")]
        [InlineData("jpeg")]
        public void TestThatForSmallEnoughImageFilesPreviewImageIsLoaded(string imageFormat)
        {
            string smallEnoughImageFilePath = $@"{Environment.CurrentDirectory}\..\..\..\ModelsTests\IO\SmallEnoughImage.{imageFormat}";
            RecentlyOpenedDocument recentlyOpenedDocument = new RecentlyOpenedDocument(smallEnoughImageFilePath);

            var smallEnoughImagePreviewImage = recentlyOpenedDocument.PreviewBitmap;

            Assert.NotNull(smallEnoughImagePreviewImage);
        }
    }
}