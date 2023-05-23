using PixiEditor.Extensions.Palettes;

namespace SampleExtension;

public class TestPaletteDataSource : PaletteListDataSource
{
    private List<ExtensionPalette> palettes = new()
    {
        new ExtensionPalette("Test Palette", new List<PaletteColor> { PaletteColor.Black, PaletteColor.White, })
    };
    public override Task<List<IPalette>> FetchPaletteList(int startIndex, int items, FilteringSettings filtering)
    {
        if(startIndex >= palettes.Count) return Task.FromResult(new List<IPalette>());

        return Task.FromResult(palettes.Skip(startIndex).Take(items).Where(filtering.Filter).Cast<IPalette>().ToList());
    }
}
