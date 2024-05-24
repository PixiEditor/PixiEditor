using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.IO;
using PixiEditor.Extensions.Palettes;

namespace PixiEditor.Extensions;

public class ExtensionServices
{
    public IServiceProvider Services { get; private set; }
    public IWindowProvider? Windowing => Services.GetService<IWindowProvider>();
    public IPaletteProvider? PaletteProvider => Services.GetService<IPaletteProvider>();
    public IFileSystemProvider? FileSystem => Services.GetService<IFileSystemProvider>();
    public IPreferences? Preferences => Services.GetService<IPreferences>();

    public ExtensionServices(IServiceProvider services)
    {
        Services = services;
    }
}
