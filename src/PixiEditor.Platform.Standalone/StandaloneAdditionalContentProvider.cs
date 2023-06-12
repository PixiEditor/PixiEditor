namespace PixiEditor.Platform.Standalone;

public sealed class StandaloneAdditionalContentProvider : IAdditionalContentProvider
{
    public bool IsContentAvailable(AdditionalContentProduct product)
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}
