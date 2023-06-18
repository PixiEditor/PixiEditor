namespace PixiEditor.Platform;

public interface IPlatform
{
    public static IPlatform Current { get; private set; }
    public abstract string Id { get; }
    public abstract string Name { get; }
    public bool PerformHandshake();
    public IAdditionalContentProvider? AdditionalContentProvider { get; }

    public static void RegisterPlatform(IPlatform platform)
    {
        if (Current != null)
        {
            throw new InvalidOperationException("Platform already initialized.");
        }

        Current = platform;
    }
}
