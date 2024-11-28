namespace PixiEditor.Extensions.CommonApi.Palettes;

public interface IPalette
{
    public string Name { get; }
    public List<PaletteColor> Colors { get; }
    public bool IsFavourite { get; }
    public string FileName { get; set; }
    public PaletteListDataSource Source { get; }
}
