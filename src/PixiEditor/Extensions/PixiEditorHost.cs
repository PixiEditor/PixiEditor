using PixiEditor.Extensions.Runtime;

namespace PixiEditor.Extensions;

public class PixiEditorHost : IHost
{
    public string HostName => "PixiEditor";
    public Version Version { get; }

    public PixiEditorHost()
    {
        Version = typeof(PixiEditorHost).Assembly.GetName().Version ?? throw new InvalidOperationException("Could not get PixiEditor version.");
    }
}
