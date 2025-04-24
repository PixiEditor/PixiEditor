using PixiEditor.IdentityProvider;
using Steamworks;
using Timer = System.Timers.Timer;

namespace PixiEditor.Platform.Steam;

public class SteamPlatform : IPlatform
{
    public string Id { get; } = "steam";
    public string Name => "Steam";

    public bool PerformHandshake()
    {
        try
        {
            bool initialized = SteamAPI.Init();
            if (!initialized) return false;

            IdentityProvider?.Initialize();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Update()
    {
        SteamAPI.RunCallbacks();
    }

    public IAdditionalContentProvider? AdditionalContentProvider { get; } = new SteamAdditionalContentProvider();
    public IIdentityProvider? IdentityProvider { get; } = new SteamIdentityProvider();
}
