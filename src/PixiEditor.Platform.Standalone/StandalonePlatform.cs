namespace PixiEditor.Platform.Standalone;

public sealed class StandalonePlatform : IPlatform
{
    public string Name => "Standalone";

    public bool PerformHandshake()
    {
        return true;
    }

    public IAdditionalContentProvider? AdditionalContentProvider { get; } = new StandaloneAdditionalContentProvider();
}
