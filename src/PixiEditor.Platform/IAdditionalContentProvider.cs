namespace PixiEditor.Platform;

public interface IAdditionalContentProvider
{
    public Task<string?> InstallContent(string productId);
    public bool IsContentOwned(string productId);
    public bool PlatformHasContent(string productId);

    public event Action<string, object> OnError;
}
