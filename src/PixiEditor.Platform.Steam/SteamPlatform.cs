using PixiEditor.IdentityProvider;
using Steamworks;
using Timer = System.Timers.Timer;

namespace PixiEditor.Platform.Steam;

public class SteamPlatform : IPlatform
{
    public static readonly AppId_t AppId = new AppId_t(2435860);
    
    public string Id { get; } = "steam";
    public string Name => "Steam";

    public IAdditionalContentProvider? AdditionalContentProvider => steamProvider;
    public IIdentityProvider? IdentityProvider { get; }

    private readonly SteamAdditionalContentProvider steamProvider;
    
    public SteamPlatform(string[] extensionsPaths)
    {
        IdentityProvider = new SteamIdentityProvider();
        steamProvider = new SteamAdditionalContentProvider(extensionsPaths);
    }

    public bool PerformHandshake()
    {
        try
        {
            Console.WriteLine("Initializing Steam API...");
            bool initialized = SteamAPI.Init();
            Console.WriteLine($"Steam API initialized: {initialized}");
            if (!initialized) return false;

            if (OperatingSystem.IsMacOS())
            {
                var paths = new List<string>(steamProvider.ExtensionsPaths);
                SteamApps.GetAppInstallDir(AppId, out string path, 4096);
                if (!string.IsNullOrEmpty(path))
                {
                    paths.Add(Path.Combine(path, "Extensions"));
                    steamProvider.ExtensionsPaths = paths.ToArray();
                }
            }

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
}
