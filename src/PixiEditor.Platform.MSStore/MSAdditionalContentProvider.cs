namespace PixiEditor.Platform.MSStore;

public sealed class MSAdditionalContentProvider : IAdditionalContentProvider
{
    public bool IsContentInstalled(AdditionalContentProduct product)
    {
        if(!PlatformHasContent(product)) return false;

        return product switch
        {
            AdditionalContentProduct.SupporterPack => false,
            _ => false
        };
    }

    public bool PlatformHasContent(AdditionalContentProduct product)
    {
        return product switch
        {
            AdditionalContentProduct.SupporterPack => false,
            _ => false
        };
    }
}
