namespace PixiEditor.Platform.Steam;

public sealed class SteamAdditionalContentProvider : IAdditionalContentProvider
{
    public bool IsContentAvailable(AdditionalContentProduct product)
    {
        return true;
    }
}
