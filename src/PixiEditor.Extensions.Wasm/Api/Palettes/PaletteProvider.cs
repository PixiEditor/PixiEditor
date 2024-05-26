using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Palettes;

namespace PixiEditor.Extensions.Wasm.Api.Palettes;

public class PaletteProvider : IPaletteProvider
{
    public AsyncCall<List<IPalette>> FetchPalettes(int startIndex, int items, FilteringSettings filtering)
    {
        throw new NotImplementedException();
    }

    public AsyncCall<bool> AddPalette(IPalette palette, bool overwrite = false)
    {
        throw new NotImplementedException();
    }

    public void RegisterDataSource(PaletteListDataSource dataSource)
    {
        throw new NotImplementedException();
    }
}
