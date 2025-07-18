using PixiEditor.Extensions.CommonApi.User;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.SubViewModels.AdditionalContent;

namespace PixiEditor.Models.ExtensionServices;

internal class UserDataProvider : IUserDataProvider
{
    public UserViewModel UserViewModel { get; }
    public AdditionalContentViewModel AdditionalContentViewModel { get; }
    public bool IsLoggedIn => UserViewModel.IsLoggedIn;
    public string Username => UserViewModel.Username;
    public string AccountProviderName => UserViewModel.IdentityProvider?.ProviderName ?? string.Empty;

    public event Action? UserLoggedIn;
    public event Action? UserLoggedOut;

    public UserDataProvider(UserViewModel userViewModel, AdditionalContentViewModel additionalContentViewModel)
    {
        UserViewModel = userViewModel;
        AdditionalContentViewModel = additionalContentViewModel;
        if (UserViewModel.IdentityProvider == null)
        {
            return;
        }

        UserViewModel.IdentityProvider.OnLoggedIn += (u) =>
        {
            UserLoggedIn?.Invoke();
        };

        UserViewModel.IdentityProvider.LoggedOut += () =>
        {
            UserLoggedOut?.Invoke();
        };
    }

    public string[] GetOwnedContent()
    {
        return UserViewModel.IdentityProvider?.User?.OwnedProducts?.Select(x => x.Id).ToArray() ?? [];
    }
}
