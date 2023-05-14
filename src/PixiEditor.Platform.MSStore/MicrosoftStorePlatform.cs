namespace PixiEditor.Platform.MSStore;

public sealed class MicrosoftStorePlatform : IPlatform
{
    public string Name => "Microsoft Store";

    public bool PerformHandshake()
    {
        return true;
    }

    public IAdditionalContentProvider? AdditionalContentProvider { get; } = new MSAdditionalContentProvider();
}
