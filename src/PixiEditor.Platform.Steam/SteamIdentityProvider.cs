using System.Runtime.InteropServices;
using PixiEditor.IdentityProvider;
using SkiaSharp;
using Steamworks;

namespace PixiEditor.Platform.Steam;

public class SteamIdentityProvider : IIdentityProvider
{
    public bool AllowsLogout { get; } = false;
    public string ProviderName { get; } = "Steam";
    public IUser User { get; private set; }
    public bool IsLoggedIn { get; private set; }
    public Uri? EditProfileUrl { get; } = new Uri("https://store.steampowered.com/login/");
    public bool IsValid
    {
        get
        {
            try
            {
                return SteamAPI.IsSteamRunning() && User != null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

    public string InvalidInfo { get; } = "STEAM_OFFLINE";
    public event Action<string, object>? OnError;
    public event Action<List<ProductData>>? OwnedProductsUpdated;
    public event Action<string>? UsernameUpdated;

    public void Initialize()
    {
        string username = SteamFriends.GetPersonaName();
        var id = Steamworks.SteamUser.GetSteamID();
        string avatar = GetAvatar(id);
        var ownedContent = GetOwnedDlcs();
        var user = new SteamUser()
        {
            Username = username, AvatarUrl = avatar, Id = id.m_SteamID, IsLoggedIn = true,
            OwnedProducts = ownedContent,
        };

        User = user;
        IsLoggedIn = true;
    }

    public event Action<IUser>? OnLoggedIn;
    public event Action? LoggedOut;

    private static string GetAvatar(CSteamID id)
    {
        int avatar = SteamFriends.GetLargeFriendAvatar(id);

        string cache = Path.Combine(Path.GetTempPath(), "PixiEditor", $"SteamAvatar_{id.m_SteamID}.png");

        bool cacheExists = File.Exists(cache);

        if (cacheExists)
        {
            return cache;
        }

        if (avatar != 0)
        {
            SteamUtils.GetImageSize(avatar, out uint width, out uint height);
            byte[] image = new byte[width * height * 4];
            bool gotImage = SteamUtils.GetImageRGBA(avatar, image, image.Length);
            if (gotImage)
            {
                using SKBitmap bitmap = new SKBitmap((int)width, (int)height);
                var allocated = GCHandle.Alloc(image, GCHandleType.Pinned);
                var info = new SKImageInfo((int)width, (int)height, SKColorType.Rgba8888, SKAlphaType.Premul);
                bitmap.InstallPixels(info, allocated.AddrOfPinnedObject(), bitmap.RowBytes, delegate { allocated.Free(); }, null);
                using FileStream stream = new FileStream(cache, FileMode.Create);
                bitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
            }
        }

        return cache;
    }

    private static List<ProductData> GetOwnedDlcs()
    {
        int dlcCount = SteamApps.GetDLCCount();

        List<ProductData> ownedDlcs = new List<ProductData>();
        for (int i = 0; i < dlcCount; i++)
        {
            bool success = SteamApps.BGetDLCDataByIndex(i, out AppId_t appId, out bool available, out string name, 128);
            if (success && available)
            {
                ownedDlcs.Add(new ProductData(appId.m_AppId.ToString(), name));
            }
        }

        return ownedDlcs;
    }
}
