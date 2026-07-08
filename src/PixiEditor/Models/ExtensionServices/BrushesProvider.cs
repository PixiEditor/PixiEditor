using PixiEditor.Extensions.CommonApi.Brushes;
using PixiEditor.Models.Controllers;

namespace PixiEditor.Models.ExtensionServices;

internal class BrushesProvider : IBrushProvider
{
    private BrushLibrary brushLibrary;

    public BrushesProvider(BrushLibrary library)
    {
        this.brushLibrary = library;
    }

    public void RegisterBrushDataSource(IBrushDataSource dataSource)
    {
        brushLibrary.RegisterExternalBrushes(dataSource);
    }
}
