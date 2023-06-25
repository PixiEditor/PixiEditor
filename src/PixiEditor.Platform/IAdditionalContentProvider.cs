namespace PixiEditor.Platform;

public enum AdditionalContentProduct
{
    SupporterPack
}

public interface IAdditionalContentProvider
{
    public bool IsContentInstalled(AdditionalContentProduct product);
    public bool PlatformHasContent(AdditionalContentProduct product);
}
