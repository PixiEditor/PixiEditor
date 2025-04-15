namespace PixiEditor.Models.Auth;

public record User
{
    public string Email { get; set; } = string.Empty;
    public string? SessionToken { get; set; } = string.Empty;
}
