namespace PixiEditor.Extensions.CommonApi.Palettes;

public partial class ExtensionPalette : IPalette
{
    public PaletteListDataSource Source { get; set; }
    
    public ExtensionPalette(string name, List<PaletteColor> colors, PaletteListDataSource source)
    {
        Name = name;
        Colors = colors;
        Source = source;
    }
    
    public ExtensionPalette()
    {
    }
}
