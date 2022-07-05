using PixiEditor.Models.DataHolders.Palettes;
using PixiEditor.Models.IO;

namespace PixiEditor.Models.DataProviders;

internal abstract class PaletteListDataSource
{
    public virtual void Initialize() { }
    public abstract Task<PaletteList> FetchPaletteList(int startIndex, int items, FilteringSettings filtering);
    public List<PaletteFileParser> AvailableParsers { get; set; }

}
