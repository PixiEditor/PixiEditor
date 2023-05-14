namespace PixiEditor.Platform;

public interface IPlatform
{
    public static IPlatform Current { get; }
    public bool PerformHandshake();
    public IAdditionalContentProvider? AdditionalContentProvider { get; }
}
