namespace PixiEditor.Platform;

public enum AdditionalContentProduct
{
    SupporterPack
}

public interface IAdditionalContentProvider
{
    public bool IsContentAvailable(AdditionalContentProduct product);
}
