using Steamworks;

namespace PixiEditor.Platform.Steam;

public sealed class SteamAdditionalContentProvider : IAdditionalContentProvider
{
    private Dictionary<AdditionalContentProduct, AppId_t> productIds = new()
    {
        { AdditionalContentProduct.SupporterPack, new AppId_t(2435860) }
    };

    public bool IsContentInstalled(AdditionalContentProduct product)
    {
        if(!SteamAPI.IsSteamRunning()) return false;
        if(!PlatformHasContent(product)) return false;

        AppId_t appId = productIds[product];
        bool installed = SteamApps.BIsDlcInstalled(appId);
        return installed;
    }

    public bool PlatformHasContent(AdditionalContentProduct product)
    {
        return productIds.ContainsKey(product);
    }
}
