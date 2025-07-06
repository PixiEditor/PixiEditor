using System.Reflection;
using Steamworks;

namespace PixiEditor.Platform.Steam;

public sealed class SteamAdditionalContentProvider : IAdditionalContentProvider
{
    Dictionary<string, AppId_t> dlcMap = new() { { "pixieditor.founderspack", new AppId_t(2435860) }, };

    public bool IsContentOwned(string product)
    {
        if (!SteamAPI.IsSteamRunning()) return false;
        if (string.IsNullOrEmpty(product)) return false;

        string productLower = product.ToLowerInvariant();

        if (!PlatformHasContent(productLower)) return false;

        AppId_t appId = new AppId_t(0);
        if (dlcMap.TryGetValue(productLower, out var value))
        {
            appId = value;
        }
        else if (!uint.TryParse(productLower, out uint id))
        {
            OnError?.Invoke("INVALID_PRODUCT_ID", product);
            return false;
        }
        else
        {
            appId = new AppId_t(id);
        }

        bool installed = SteamApps.BIsDlcInstalled(appId);
        return installed;
    }

    public async Task<string?> InstallContent(string productId)
    {
        if (string.IsNullOrEmpty(productId)) return null;

        string productLower = productId.ToLowerInvariant();

        if (!SteamAPI.IsSteamRunning()) return null;
        if (!PlatformHasContent(productLower)) return null;

        AppId_t appId = new AppId_t(0);
        if (dlcMap.TryGetValue(productLower, out var value))
        {
            appId = value;
        }
        else if (!uint.TryParse(productLower, out uint id))
        {
            OnError?.Invoke("INVALID_PRODUCT_ID", productId);
            return null;
        }
        else
        {
            appId = new AppId_t(id);
        }

        if (SteamApps.BIsDlcInstalled(appId))
        {
            OnError?.Invoke("ALREADY_INSTALLED", productId);
            return null;
        }

        SteamApps.GetAppInstallDir(new AppId_t(2218560), out string appInstallDir, 260);

#if DEBUG
        appInstallDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#endif

        string extensionsDir = Path.Combine(appInstallDir, "Extensions");

        string[] activeExtensions = Directory.GetFiles(extensionsDir, "*.pixiext");

        SteamApps.InstallDLC(appId);

        while (!SteamApps.BIsDlcInstalled(appId))
        {
            await Task.Delay(500);
        }

        string[] installedExtensions = Directory.GetFiles(extensionsDir, "*.pixiext");

        string[] newExtensions = installedExtensions.Except(activeExtensions).ToArray();

        if (newExtensions.Length == 0)
        {
            OnError?.Invoke("UNABLE_TO_FIND_EXTENSION", productId);
            return null;
        }

        string extensionPath = newExtensions[0];

        return extensionPath;
    }

    public bool PlatformHasContent(string product)
    {
        if (dlcMap.ContainsKey(product))
        {
            return true;
        }

        bool isValid = uint.TryParse(product, out uint id);
        if (!isValid)
        {
            return false;
        }

        AppId_t appId = new AppId_t(id);
        int dlcs = SteamApps.GetDLCCount();
        for (int i = 0; i < dlcs; i++)
        {
            SteamApps.BGetDLCDataByIndex(i, out AppId_t dlcId, out bool available, out string name, 128);
            if (appId == dlcId)
            {
                return true;
            }
        }

        return false;
    }

    public event Action<string, object>? OnError;

    public bool IsInstalled(string productId)
    {
        if (string.IsNullOrEmpty(productId)) return false;

        string productIdLower = productId.ToLowerInvariant();

        AppId_t appId = new AppId_t(0);
        if (dlcMap.TryGetValue(productIdLower, out var value))
        {
            appId = value;
        }
        else if (!uint.TryParse(productIdLower, out uint id))
        {
            return false;
        }
        else
        {
            appId = new AppId_t(id);
        }

        return SteamApps.BIsDlcInstalled(appId);
    }
}
