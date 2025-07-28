using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;

namespace PixiEditor.Extensions.CommonApi.Palettes;

public abstract class PaletteListDataSource
{
    public string Name { get; set; }

    public PaletteListDataSource(string name)
    {
        Name = name;
        AvailableParsers = new List<PaletteFileParser>();
    }

    public virtual void Initialize() { }

    /// <summary>
    ///     Fetches palettes from the provider.
    /// </summary>
    /// <param name="startIndex">Starting fetch index. Palettes before said index won't be fetched.</param>
    /// <param name="items">Max amount of palettes to fetch.</param>
    /// <param name="filtering">Filtering settings for fetching.</param>
    /// <returns>A List of palettes. Null if fetch wasn't successful.</returns>
    public abstract AsyncCall<List<IPalette>> FetchPaletteList(int startIndex, int items, FilteringSettings filtering);
    public List<PaletteFileParser> AvailableParsers { get; set; }
}
