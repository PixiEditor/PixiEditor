namespace PixiEditor.IdentityProvider;

public interface IUser
{
    public string Username { get; }
    public string? AvatarUrl { get; }
    public List<ProductData> OwnedProducts { get; }
    public bool IsLoggedIn { get; }
}
