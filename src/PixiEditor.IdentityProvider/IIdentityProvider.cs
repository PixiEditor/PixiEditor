namespace PixiEditor.IdentityProvider;

public interface IIdentityProvider
{
    public IUser User { get; }
    public bool IsLoggedIn { get; }

    public event Action<string, object> OnError;
    public event Action<List<string>> OwnedProductsUpdated;
    public event Action<string> UsernameUpdated;
}
