using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Extensions.Windowing;

namespace PixiEditor.Extensions;

public class ExtensionServices
{
    public IServiceProvider Services { get; private set; }
    public IWindowProvider WindowProvider => Services.GetRequiredService<IWindowProvider>();
    public IPaletteProvider PaletteProvider => Services.GetRequiredService<IPaletteProvider>();

    public ExtensionServices(IServiceProvider services)
    {
        Services = services;
    }
}
