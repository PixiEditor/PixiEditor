using PixiEditor.IdentityProvider;
using PixiEditor.IdentityProvider.PixiAuth;
using PixiEditor.PixiAuth;

namespace PixiEditor.Platform.Standalone;

public sealed class StandalonePlatform : IPlatform
{
    public string Id { get; } = "standalone";
    public string Name => "Standalone";

    public IIdentityProvider? IdentityProvider { get; }
    public IAdditionalContentProvider? AdditionalContentProvider { get; }

    public StandalonePlatform(string extensionsPath, string apiUrl, string? apiKey)
    {
        PixiAuthIdentityProvider authProvider = new PixiAuthIdentityProvider(apiUrl, apiKey);
        IdentityProvider = authProvider;
        AdditionalContentProvider = new StandaloneAdditionalContentProvider(extensionsPath, authProvider);
    }

    public bool PerformHandshake()
    {
        IdentityProvider?.Initialize();
        return true;
    }

    public void Update()
    {
    }
}
