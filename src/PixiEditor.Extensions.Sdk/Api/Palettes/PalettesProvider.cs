using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.Palettes;

public class PalettesProvider : IPalettesProvider
{
    public void RegisterDataSource(PaletteListDataSource dataSource)
    {
        Interop.RegisterDataSource(dataSource);
    }
}
