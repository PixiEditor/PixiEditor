namespace PixiEditor.PixiAuth;

[Serializable]
public class User
{
    public string EmailHash { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public Guid? SessionId { get; set; }
    public string? SessionToken { get; set; } = string.Empty;
    public DateTime? SessionExpirationDate { get; set; }

    public List<string> OwnedProducts { get; set; } = new();

    public User()
    {

    }
}
