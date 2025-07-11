namespace PixiEditor.Extensions.CommonApi.User;

public interface IUserDataProvider
{
    public bool IsLoggedIn { get; }
    public string Username { get; }
    public string AccountProviderName { get; }
    public string[] GetOwnedContent();
}
