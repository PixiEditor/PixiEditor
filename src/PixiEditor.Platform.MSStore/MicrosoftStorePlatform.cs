namespace PixiEditor.Platform.MSStore;

public sealed class MicrosoftStorePlatform : IPlatform
{
    public bool PerformHandshake()
    {
        return true;
    }

    public IAdditionalContentProvider? AdditionalContentProvider { get; } = new MSAdditionalContentProvider();
}
