namespace PixiEditor.Extensions.CommonApi.Palettes;

public interface IPalettesProvider
{
    /// <summary>
    ///     Registers a data source of palettes.
    /// </summary>
    /// <param name="dataSource">Palettes data source</param>
    public void RegisterDataSource(PaletteListDataSource dataSource);
}
