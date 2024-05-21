namespace PixiEditor.Platform.Standalone;

public sealed class StandaloneAdditionalContentProvider : IAdditionalContentProvider
{
    public bool IsContentInstalled(AdditionalContentProduct product)
    {
        if(!PlatformHasContent(product)) return false;
#if DEBUG
        return true;
#else
        return false;
#endif
    }

    public bool PlatformHasContent(AdditionalContentProduct product)
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}
