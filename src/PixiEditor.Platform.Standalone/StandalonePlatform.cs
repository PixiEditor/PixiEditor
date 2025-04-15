namespace PixiEditor.Platform.Standalone;

public sealed class StandalonePlatform : IPlatform
{
    public string Id { get; } = "standalone";
    public string Name => "Standalone";

    public bool PerformHandshake()
    {

        return true;
    }

    public void Update()
    {

    }

    public IAdditionalContentProvider? AdditionalContentProvider { get; } = new StandaloneAdditionalContentProvider();
}
