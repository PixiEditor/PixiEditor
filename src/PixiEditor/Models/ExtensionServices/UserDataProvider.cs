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

    public UserDataProvider(UserViewModel userViewModel, AdditionalContentViewModel additionalContentViewModel)
    {
        UserViewModel = userViewModel;
        AdditionalContentViewModel = additionalContentViewModel;
    }

    public string[] GetOwnedContent()
    {
        return UserViewModel.IdentityProvider?.User?.OwnedProducts?.Select(x => x.Id).ToArray() ?? [];
    }
}
