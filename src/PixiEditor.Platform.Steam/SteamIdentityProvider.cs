using PixiEditor.IdentityProvider;

namespace PixiEditor.Platform.Steam;

public class SteamIdentityProvider : IIdentityProvider
{
    public IUser User { get; } // TODO: Implement
    public bool IsLoggedIn { get; }
    public event Action<string, object>? OnError;
    public event Action<List<string>>? OwnedProductsUpdated;
    public event Action<string>? UsernameUpdated;
}
