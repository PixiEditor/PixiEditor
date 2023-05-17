using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions.Windowing;

namespace PixiEditor.Extensions;

public class ExtensionServices
{
    public ServiceProvider Services { get; private set; }

    public IWindowProvider WindowProvider => Services.GetRequiredService<IWindowProvider>();

    public ExtensionServices(ServiceProvider services)
    {
        Services = services;
    }
}
