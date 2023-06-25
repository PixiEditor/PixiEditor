namespace PixiEditor.Extensions.Palettes;

public class ExtensionPalette : IPalette
{
    public string Name { get; }
    public List<PaletteColor> Colors { get; }
    public bool IsFavourite { get; set; }
    public string FileName { get; set; }
    public PaletteListDataSource Source { get; }

    public ExtensionPalette(string name, List<PaletteColor> colors, PaletteListDataSource source)
    {
        Name = name;
        Colors = colors;
        Source = source;
    }
}
