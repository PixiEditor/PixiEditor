using Xunit;

namespace PixiEditorTests.ModelsTests.LospecPaletteFetcher
{
    public class LospecPaletteFetcherTests
    {
        [Fact]
        public async void TestThatLospecPaletteFetcherFetchesData()
        {
            var result = await PixiEditor.Models.ExternalServices.LospecPaletteFetcher.FetchPage(0);
            Assert.NotEmpty(result.Palettes);
        }
    }
}
