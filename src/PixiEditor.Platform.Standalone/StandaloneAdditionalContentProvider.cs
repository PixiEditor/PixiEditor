namespace PixiEditor.Platform.Standalone;

public sealed class StandaloneAdditionalContentProvider : IAdditionalContentProvider
{
    public bool IsContentAvailable(AdditionalContentProduct product)
    {
        return false;
    }
}
