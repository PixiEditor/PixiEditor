using Steamworks;

namespace PixiEditor.Platform.Steam;

public sealed class SteamAdditionalContentProvider : IAdditionalContentProvider
{
    private Dictionary<AdditionalContentProduct, AppId_t> productIds = new()
    {
        { AdditionalContentProduct.SupporterPack, new AppId_t(2435860) }
    };

    public bool IsContentAvailable(AdditionalContentProduct product)
    {
        if(!productIds.ContainsKey(product)) return false;

        AppId_t appId = productIds[product];
        return SteamApps.BIsDlcInstalled(appId);
    }
}
