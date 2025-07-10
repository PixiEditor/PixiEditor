using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions.Commands;
using PixiEditor.Extensions.CommonApi.Commands;
using PixiEditor.Extensions.CommonApi.IO;
using PixiEditor.Extensions.CommonApi.Logging;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Ui;
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
    public IDocumentProvider Documents => Services.GetService<IDocumentProvider>();
    public ICommandSupervisor CommandSupervisor => Services.GetService<ICommandSupervisor>();
    public IVisualTreeProvider VisualTree => Services.GetService<IVisualTreeProvider>();
    public ILogger Logger => Services.GetService<ILogger>();

    public ExtensionServices(IServiceProvider services)
    {
        Services = services;
    }
}
