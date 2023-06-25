namespace PixiEditor.Platform.Standalone;

public sealed class StandaloneAdditionalContentProvider : IAdditionalContentProvider
{
    public bool IsContentInstalled(AdditionalContentProduct product)
    {
        //if(!PlatformHasContent(product)) return false;
        return false;
    }

    public bool PlatformHasContent(AdditionalContentProduct product)
    {
        return false;
    }
}
