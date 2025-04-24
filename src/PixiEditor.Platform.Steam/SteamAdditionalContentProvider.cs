using Steamworks;

namespace PixiEditor.Platform.Steam;

public sealed class SteamAdditionalContentProvider : IAdditionalContentProvider
{
    private Dictionary<string, AppId_t> productIds = new()
    {
        { "PixiEditor.FoundersPack", new AppId_t(2435860) }
    };

    public bool IsContentOwned(string product)
    {
        if(!SteamAPI.IsSteamRunning()) return false;
        if(!PlatformHasContent(product)) return false;

        AppId_t appId = productIds[product];
        bool installed = SteamApps.BIsDlcInstalled(appId);
        return installed;
    }

    public async Task<string?> InstallContent(string productId)
    {
        if (!SteamAPI.IsSteamRunning()) return null;
        if (!PlatformHasContent(productId)) return null;

        AppId_t appId = productIds[productId];
        SteamApps.InstallDLC(appId);

        // Steam does not provide a way to check if the installation was successful
        // so we will just return the product ID
        // TODO: Implement properly
        return productId;
    }

    public bool PlatformHasContent(string product)
    {
        return productIds.ContainsKey(product);
    }

    public event Action<string, object>? OnError;
}
