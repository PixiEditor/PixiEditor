using PixiEditor.IdentityProvider;

namespace PixiEditor.Platform;

public interface IPlatform
{
    public static IPlatform Current { get; private set; }
    public abstract string Id { get; }
    public abstract string Name { get; }
    public bool PerformHandshake();
    public void Update();
    public IAdditionalContentProvider? AdditionalContentProvider { get; }
    public IIdentityProvider? IdentityProvider { get; }

    public static void RegisterPlatform(IPlatform platform)
    {
        if (Current != null)
        {
            throw new InvalidOperationException("Platform already initialized.");
        }

        Current = platform;
    }
}

public class NullAdditionalContentProvider : IAdditionalContentProvider
{
    public Task<string?> InstallContent(string productId)
    {
        return Task.FromResult<string?>(null);
    }

    public bool IsContentOwned(string productId)
    {
        return false;
    }

    public bool PlatformHasContent(string productId)
    {
        return false;
    }

    public event Action<string, object>? OnError;
    public bool IsInstalled(string productId)
    {
        return false;
    }
}
