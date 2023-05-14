namespace PixiEditor.Platform.Standalone;

public sealed class StandalonePlatform : IPlatform
{
    public bool PerformHandshake()
    {
        return true;
    }

    public IAdditionalContentProvider? AdditionalContentProvider { get; } = new StandaloneAdditionalContentProvider();
}
