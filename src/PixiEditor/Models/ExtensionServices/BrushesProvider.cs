using PixiEditor.Extensions.CommonApi.Brushes;
using PixiEditor.Models.Controllers;

namespace PixiEditor.Models.ExtensionServices;

internal class BrushesProvider : IBrushProvider
{
    public BrushLibrary library;

    public BrushesProvider(BrushLibrary library)
    {
        this.library = library;
    }

    public void RegisterBrushDataSource(IBrushDataSource dataSource)
    {
        library.RegisterExternalBrushes(dataSource);
    }
}
