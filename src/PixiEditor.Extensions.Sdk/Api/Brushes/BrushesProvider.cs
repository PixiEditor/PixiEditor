using PixiEditor.Extensions.CommonApi.Brushes;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.Brushes;

public class BrushesProvider : IBrushProvider
{
    public void RegisterBrushDataSource(IBrushDataSource dataSource)
    {
        Interop.RegisterDataSource(dataSource);
    }
}
