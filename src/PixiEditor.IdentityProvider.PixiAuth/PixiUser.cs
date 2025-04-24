namespace PixiEditor.IdentityProvider.PixiAuth;

[Serializable]
public class PixiUser : IUser
{
    public string Username { get; set; }

    public string? AvatarUrl =>
        EmailHash != null ? $"https://www.gravatar.com/avatar/{EmailHash}?s=100&d=initials" : null;

    public string EmailHash { get; set; } = string.Empty;
    public List<string> OwnedProducts { get; set; }
    public Guid? SessionId { get; set; }
    public string? SessionToken { get; set; } = string.Empty;
    public DateTime? SessionExpirationDate { get; set; }

    public bool IsLoggedIn => this is { SessionId: not null } && !string.IsNullOrEmpty(SessionToken);
    public bool IsWaitingForActivation => this is { SessionId: not null } && string.IsNullOrEmpty(SessionToken);
}
