using PixiEditor.Extensions.Metadata;

namespace PixiEditor.Extensions.Runtime;

internal class DiscoveredExtension
{
    public ExtensionMetadata Metadata { get; init; }
    public string PackagePath { get; init; }
    public bool Disabled { get; set; }
}
