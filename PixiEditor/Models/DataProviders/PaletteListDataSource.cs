using PixiEditor.Models.DataHolders.Palettes;
using PixiEditor.Models.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PixiEditor.Models.DataProviders
{
    public abstract class PaletteListDataSource
    {
        public virtual void Initialize() { }
        public abstract Task<PaletteList> FetchPaletteList(int startIndex, int items);
        public List<PaletteFileParser> AvailableParsers { get; set; }

    }
}
