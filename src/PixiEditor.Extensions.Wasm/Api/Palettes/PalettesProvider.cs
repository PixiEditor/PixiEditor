using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.Wasm.Bridge;

namespace PixiEditor.Extensions.Wasm.Api.Palettes;

public class PalettesProvider : IPalettesProvider
{
    public void RegisterDataSource(PaletteListDataSource dataSource)
    {
        Interop.RegisterDataSource(dataSource);
    }
}
