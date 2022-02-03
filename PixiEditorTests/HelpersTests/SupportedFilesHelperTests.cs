using PixiEditor.Helpers;
using Xunit;

namespace PixiEditorTests.HelpersTests
{
    public class SupportedFilesHelperTests
    {
        [Fact]
        public void TestAllExtensionsAreSupported()
        {
            var all = SupportedFilesHelper.GetAllSupportedExtensions();
            Assert.Contains(all, i => i == ".pixi");
            Assert.Contains(all, i => i == ".png");
            Assert.Contains(all, i => i == ".jpg");
            Assert.Contains(all, i => i == ".jpeg");
            Assert.Contains(all, i => i == ".bmp");
            Assert.Contains(all, i => i == ".gif");
        }

        [Fact]
        public void TestBuildSaveFilter()
        {
            var filter = SupportedFilesHelper.BuildSaveFilter(true);
            Assert.Equal("PixiEditor Files|*.pixi|Png Images|*.png|Jpeg Images|*.jpeg|Bmp Images|*.bmp|Gif Images|*.gif", filter);
        }

        [Fact]
        public void TestBuildOpenFilter()
        {
            var filter = SupportedFilesHelper.BuildOpenFilter();
            Assert.Equal("Any |*.pixi;*.png;*.jpeg;*.jpg;*.bmp;*.gif|PixiEditor Files |*.pixi|Image Files |*.png;*.jpeg;*.jpg;*.bmp;*.gif", filter);
        }

        [Fact]
        public void TestIsSupportedFile()
        {
            Assert.True(SupportedFilesHelper.IsSupportedFile("foo.png"));
            Assert.True(SupportedFilesHelper.IsSupportedFile("foo.bmp"));
            Assert.True(SupportedFilesHelper.IsSupportedFile("foo.jpg"));
            Assert.True(SupportedFilesHelper.IsSupportedFile("foo.jpeg"));

            Assert.False(SupportedFilesHelper.IsSupportedFile("foo.abc"));
        }
    }
}
