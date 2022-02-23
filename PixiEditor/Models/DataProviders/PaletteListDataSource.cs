using PixiEditor.Models.DataHolders.Palettes;
using System.Threading.Tasks;

namespace PixiEditor.Models.DataProviders
{
    public abstract class PaletteListDataSource
    {
        public abstract Task<PaletteList> FetchPaletteList();
    }
}
