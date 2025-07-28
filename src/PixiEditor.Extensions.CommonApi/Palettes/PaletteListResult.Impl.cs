namespace PixiEditor.Extensions.CommonApi.Palettes;

public partial class PaletteListResult
{
    public PaletteListResult()
    {
    }
    
    public PaletteListResult(IEnumerable<ExtensionPalette> palettes)
    {
        Palettes = new List<ExtensionPalette>(palettes);
    }
}
