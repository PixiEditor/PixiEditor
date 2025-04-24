namespace PixiEditor.IdentityProvider;

public interface IIdentityProvider
{
    public bool AllowsLogout { get; }
    public string ProviderName { get; }
    public IUser User { get; }
    public bool IsLoggedIn { get; }
    public Uri? EditProfileUrl { get; }

    public event Action<string, object> OnError;
    public event Action<List<ProductData>> OwnedProductsUpdated;
    public event Action<string> UsernameUpdated;

    public void Initialize();
}
