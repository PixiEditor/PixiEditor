namespace PixiEditor.Platform.Steam;

public class SteamPlatform : IPlatform
{
    public bool PerformHandshake()
    {
        return true;
    }

    public IAdditionalContentProvider? AdditionalContentProvider { get; } = new SteamAdditionalContentProvider();

    public static IPlatform Current { get; } = new SteamPlatform();
}
