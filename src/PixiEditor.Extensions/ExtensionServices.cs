using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions.CommonApi.Menu;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.IO;

namespace PixiEditor.Extensions;

public class ExtensionServices
{
    public IServiceProvider Services { get; private set; }
    public IWindowProvider? Windowing => Services.GetService<IWindowProvider>();
    public IFileSystemProvider? FileSystem => Services.GetService<IFileSystemProvider>();
    public IPreferences? Preferences => Services.GetService<IPreferences>();
    public ICommandProvider? Commands => Services.GetService<ICommandProvider>();
    
    public IPalettesProvider? Palettes => Services.GetService<IPalettesProvider>();

    public ExtensionServices(IServiceProvider services)
    {
        Services = services;
    }
}
