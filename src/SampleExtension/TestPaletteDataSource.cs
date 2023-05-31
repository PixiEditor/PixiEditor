using PixiEditor.Extensions.Palettes;

namespace SampleExtension;

public class TestPaletteDataSource : PaletteListDataSource
{
    private List<ExtensionPalette> palettes = new();

    public TestPaletteDataSource() : base("SE:DATA_SOURCE_NAME") // SE: prefix (Sample Extension:) helps to avoid key collisions with other extensions
    {
        palettes.Add(new ExtensionPalette("Test Palette", new List<PaletteColor> { PaletteColor.Black, PaletteColor.White, }, this));
    }

    public override Task<List<IPalette>> FetchPaletteList(int startIndex, int items, FilteringSettings filtering)
    {
        if(startIndex >= palettes.Count) return Task.FromResult(new List<IPalette>());

        return Task.FromResult(palettes.Skip(startIndex).Take(items).Where(filtering.Filter).Cast<IPalette>().ToList());
    }
}
