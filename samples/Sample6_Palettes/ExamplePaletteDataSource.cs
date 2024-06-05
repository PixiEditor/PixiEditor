using System.Collections.Generic;
using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.Sdk;

namespace PalettesSample;

public class ExamplePaletteDataSource : PaletteListDataSource
{
    public ExamplePaletteDataSource(string name) : base(name)
    {
    }

    public override AsyncCall<List<IPalette>> FetchPaletteList(int startIndex, int items, FilteringSettings filtering)
    {
        return AsyncCall<List<IPalette>>.FromResult([
            new ExtensionPalette("Example Palette", new List<PaletteColor>
            {
                new PaletteColor(255, 0, 0),
                new PaletteColor(0, 255, 0),
                new PaletteColor(0, 0, 255)
            }, this)
        ]);
    }
}