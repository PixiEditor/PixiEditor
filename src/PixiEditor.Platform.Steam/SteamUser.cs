using PixiEditor.IdentityProvider;

namespace PixiEditor.Platform.Steam;

public class SteamUser : IUser
{
    public ulong Id { get; set; }
    public string Username { get; set; }
    public string? AvatarUrl { get; set; }
    public List<ProductData> OwnedProducts { get; set; }
    public bool IsLoggedIn { get; set; }
}
