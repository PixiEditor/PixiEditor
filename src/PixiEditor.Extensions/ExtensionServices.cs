using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions.IO;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Extensions.Windowing;

namespace PixiEditor.Extensions;

public class ExtensionServices
{
    public IServiceProvider Services { get; private set; }
    public IWindowProvider? Windowing => Services.GetService<IWindowProvider>();
    public IPaletteProvider? PaletteProvider => Services.GetService<IPaletteProvider>();
    public IFileSystemProvider? FileSystem => Services.GetService<IFileSystemProvider>();

    public ExtensionServices(IServiceProvider services)
    {
        Services = services;
    }
}
