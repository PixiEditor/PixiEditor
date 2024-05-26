using PixiEditor.Extensions.CommonApi.Async;

namespace PixiEditor.Extensions.CommonApi.Palettes;

public interface IPaletteProvider
{
    /// <summary>
    ///     Fetches palettes from the provider.
    /// </summary>
    /// <param name="startIndex">Starting fetch index. Palettes before said index won't be fetched.</param>
    /// <param name="items">Max amount of palettes to fetch.</param>
    /// <param name="filtering">Filtering settings for fetching.</param>
    /// <returns>List of palettes.</returns>
    public AsyncCall<List<IPalette>> FetchPalettes(int startIndex, int items, FilteringSettings filtering);

    /// <summary>
    ///     Adds a palette to the provider. This means that the palette will be saved in local storage.
    /// </summary>
    /// <param name="palette">Palette to save.</param>
    /// <param name="overwrite">If true and palette with the same name exists, it will be overwritten. If false and palette with the same name exists, it will not be added.</param>
    /// <returns>True if adding palette was successful.</returns>
    public AsyncCall<bool> AddPalette(IPalette palette, bool overwrite = false);

    /// <summary>
    ///     Registers a palette list data source. This means that the provider will use the data source to fetch palettes.
    /// </summary>
    /// <param name="dataSource">Data source to register.</param>
    public void RegisterDataSource(PaletteListDataSource dataSource);
}
