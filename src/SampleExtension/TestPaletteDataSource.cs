using PixiEditor.Extensions.Palettes;

namespace SampleExtension;

public class TestPaletteDataSource : PaletteListDataSource
{
    public override Task<List<IPalette>> FetchPaletteList(int startIndex, int items, FilteringSettings filtering)
    {
        return Task.FromResult(new List<IPalette>
        {
            new ExtensionPalette("Test Palette", new List<PaletteColor>
            {
                PaletteColor.Black,
                PaletteColor.White,
            })
        });
    }
}
