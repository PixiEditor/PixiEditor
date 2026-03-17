using PixiEditor.Extensions.Metadata;

namespace PixiEditor.Extensions.Runtime;

public class DiscoveredExtension
{
    public ExtensionMetadata Metadata { get; init; }
    public string PackagePath { get; init; }
    public bool Disabled { get; set; }
}
