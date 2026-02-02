namespace PixiEditor.Platform;

public interface IAdditionalContentProvider
{
    public Task<string?> InstallContent(string productId);
    public bool IsContentOwned(string productId);
    public bool PlatformHasContent(string productId);

    public event Action<string, object> OnError;
    public bool IsInstalled(string productId);

    public Task<List<AvailableContent>> FetchAvailableExtensions();
}
