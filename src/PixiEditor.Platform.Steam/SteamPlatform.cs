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
            Console.WriteLine("Initializing Steam API...");
            bool initialized = SteamAPI.Init();
            Console.WriteLine($"Steam API initialized: {initialized}");
            if (!initialized) return false;

            IdentityProvider?.Initialize();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing Steam API: {ex.Message}");
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
