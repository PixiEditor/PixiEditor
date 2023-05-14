namespace PixiEditor.Platform.MSStore;

public sealed class MSAdditionalContentProvider : IAdditionalContentProvider
{
    public bool IsContentAvailable(AdditionalContentProduct product)
    {
        return true;
    }
}
