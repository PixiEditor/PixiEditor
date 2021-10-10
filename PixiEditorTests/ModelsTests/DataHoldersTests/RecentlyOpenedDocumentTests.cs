using PixiEditor.Models.DataHolders;
using System;
using Xunit;

namespace PixiEditorTests.ModelsTests.DataHoldersTests
{
    [Collection("Application collection")]
    public class RecentlyOpenedDocumentTests
    {
        [Fact]
        public void TestThatForBigPixiFilesPreviewImageIsNotLoaded()
        {
            string bigFilePath = $@"{Environment.CurrentDirectory}\..\..\..\ModelsTests\IO\BigPixiFile.pixi";
            RecentlyOpenedDocument recentlyOpenedDocument = new RecentlyOpenedDocument(bigFilePath);

            var bigFilePreviewImage = recentlyOpenedDocument.PreviewBitmap;

            Assert.Null(bigFilePreviewImage);
        }

        [Fact]
        public void TestThatForSmallEnoughPixiFilesPreviewImageIsLoaded()
        {
            string smallEnoughFilePath = $@"{Environment.CurrentDirectory}\..\..\..\ModelsTests\IO\SmallEnoughPixiFile.pixi";
            RecentlyOpenedDocument recentlyOpenedDocument = new RecentlyOpenedDocument(smallEnoughFilePath);

            var smallEnoughFilePreviewImage = recentlyOpenedDocument.PreviewBitmap;

            Assert.NotNull(smallEnoughFilePreviewImage);
        }
    }
}