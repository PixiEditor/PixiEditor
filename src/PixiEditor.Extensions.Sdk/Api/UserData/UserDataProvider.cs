using PixiEditor.Extensions.CommonApi.User;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.UserData;

public class UserDataProvider : IUserDataProvider
{
    public event Action UserLoggedIn;
    public event Action UserLoggedOut;

    public bool IsLoggedIn => Native.is_user_logged_in();
    public string Username => Native.get_username();
    public string AccountProviderName => Native.get_account_provider_name();
    public string[] GetOwnedContent()
    {
        return Interop.GetOwnedContent();
    }

    internal void OnUserLoggedIn()
    {
        UserLoggedIn?.Invoke();
    }

    internal void OnUserLoggedOut()
    {
        UserLoggedOut?.Invoke();
    }
}
