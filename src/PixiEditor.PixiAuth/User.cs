namespace PixiEditor.PixiAuth;

[Serializable]
public class User
{
    public string Email { get; set; } = string.Empty;
    public Guid? SessionId { get; set; }
    public string? SessionToken { get; set; } = string.Empty;
    public DateTime? SessionExpirationDate { get; set; }

    public User()
    {

    }

    public User(string email)
    {
        Email = email;
    }
}
