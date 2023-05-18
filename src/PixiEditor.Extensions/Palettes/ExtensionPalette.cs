namespace PixiEditor.Extensions.Palettes;

public class ExtensionPalette : IPalette
{
    public string Name { get; }
    public List<PaletteColor> Colors { get; }
    public bool IsFavourite { get; set; }

    public ExtensionPalette(string name, List<PaletteColor> colors)
    {
        Name = name;
        Colors = colors;
    }
}
