using Steamworks;

namespace PixiEditor.Platform.Steam;

public class SteamPlatform : IPlatform
{
    public string Name => "Steam";

    public bool PerformHandshake()
    {
        try
        {
            SteamAPI.Init();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public IAdditionalContentProvider? AdditionalContentProvider { get; } = new SteamAdditionalContentProvider();
}
