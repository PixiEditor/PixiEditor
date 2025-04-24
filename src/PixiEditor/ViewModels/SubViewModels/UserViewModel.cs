using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using DiscordRPC;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Helpers;
using PixiEditor.IdentityProvider;
using PixiEditor.IdentityProvider.PixiAuth;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.OperatingSystem;
using PixiEditor.PixiAuth;
using PixiEditor.PixiAuth.Exceptions;
using PixiEditor.PixiAuth.Utils;
using PixiEditor.Platform;

namespace PixiEditor.ViewModels.SubViewModels;

internal class UserViewModel : SubViewModel<ViewModelMain>
{
    private LocalizedString? lastError = null;

    public IIdentityProvider IdentityProvider { get; }
    public IAdditionalContentProvider AdditionalContentProvider { get; }

    public bool NotLoggedIn => !IsLoggedIn && !WaitingForActivation;

    public bool WaitingForActivation => IdentityProvider is PixiAuthIdentityProvider
    {
        User: { IsWaitingForActivation: true }
    };

    public bool IsLoggedIn => IdentityProvider.IsLoggedIn;

    public IUser User => IdentityProvider.User;

    public bool EmailEqualsLastSentMail =>
        (CurrentEmail != null ? EmailUtility.GetEmailHash(CurrentEmail) : "") == lastSentHash;

    public AsyncRelayCommand<string> RequestLoginCommand { get; }
    public AsyncRelayCommand TryValidateSessionCommand { get; }
    public AsyncRelayCommand<string> ResendActivationCommand { get; }
    public AsyncRelayCommand LogoutCommand { get; }
    public AsyncRelayCommand<string> InstallContentCommand { get; }

    private string lastSentHash = string.Empty;

    public LocalizedString? LastError
    {
        get => lastError;
        set => SetProperty(ref lastError, value);
    }


    public DateTime? TimeToEndTimeout { get; private set; } = null;

    public string TimeToEndTimeoutString
    {
        get
        {
            if (TimeToEndTimeout == null)
            {
                return string.Empty;
            }

            TimeSpan timeLeft = TimeToEndTimeout.Value - DateTime.Now;
            return timeLeft.TotalSeconds > 0 ? $"({timeLeft:ss})" : string.Empty;
        }
    }

    public ObservableCollection<string> OwnedProducts => new(IdentityProvider?.User?.OwnedProducts ?? new List<string>());

    private string currentEmail = string.Empty;

    public string CurrentEmail
    {
        get => currentEmail;
        set
        {
            if (SetProperty(ref currentEmail, value))
            {
                NotifyProperties();
            }
        }
    }

    public string Username => IdentityProvider?.User?.Username;

    public string? AvatarUrl => IdentityProvider.User?.AvatarUrl;

    public UserViewModel(ViewModelMain owner) : base(owner)
    {
        IdentityProvider = IPlatform.Current.IdentityProvider;
        AdditionalContentProvider = IPlatform.Current.AdditionalContentProvider;
        RequestLoginCommand = new AsyncRelayCommand<string>(RequestLogin, CanRequestLogin);
        TryValidateSessionCommand = new AsyncRelayCommand(TryValidateSession);
        ResendActivationCommand = new AsyncRelayCommand<string>(ResendActivation, CanResendActivation);
        InstallContentCommand = new AsyncRelayCommand<string>(InstallContent);
        LogoutCommand = new AsyncRelayCommand(Logout);

        IdentityProvider.OnError += OnError;
        IdentityProvider.OwnedProductsUpdated += IdentityProviderOnOwnedProductsUpdated;
        IdentityProvider.UsernameUpdated += IdentityProviderOnUsernameUpdated;

        if (IdentityProvider is PixiAuthIdentityProvider pixiAuth)
        {
            pixiAuth.LoginRequestSuccessful += PixiAuthOnLoginRequestSuccessful;
            pixiAuth.LoginTimeout += PixiAuthOnLoginTimeout;
            pixiAuth.LoggedOut += PixiAuthOnLoggedOut;
        }
    }

    private void IdentityProviderOnUsernameUpdated(string newUsername)
    {
        NotifyProperties();
    }

    private void IdentityProviderOnOwnedProductsUpdated(List<string> products)
    {
        NotifyProperties();
    }

    private void PixiAuthOnLoggedOut()
    {
        OwnedProducts.Clear();
        NotifyProperties();
    }

    private void PixiAuthOnLoginTimeout(double seconds)
    {
        TimeToEndTimeout = DateTime.Now.AddSeconds(seconds);
        RunTimeoutTimers(seconds);
        NotifyProperties();
    }

    private void PixiAuthOnLoginRequestSuccessful(PixiUser user)
    {
        lastSentHash = user.EmailHash;
        NotifyProperties();
    }

    public async Task RequestLogin(string email)
    {
        if (IdentityProvider is PixiAuthIdentityProvider pixiAuthIdentityProvider)
        {
            LastError = null;
            try
            {
                await pixiAuthIdentityProvider.RequestLogin(email);
            }
            catch (Exception ex)
            {
                CrashHelper.SendExceptionInfo(ex);
            }
        }
    }

    public bool CanRequestLogin(string email)
    {
        return IdentityProvider is PixiAuthIdentityProvider && !string.IsNullOrEmpty(email) && email.Contains('@');
    }

    public async Task ResendActivation(string email)
    {
        if (IdentityProvider is PixiAuthIdentityProvider pixiAuthIdentityProvider)
        {
            LastError = null;
            try
            {
                await pixiAuthIdentityProvider.ResendActivation(email);
            }
            catch (Exception ex)
            {
                CrashHelper.SendExceptionInfo(ex);
            }
        }
    }

    private void RunTimeoutTimers(double timeLeft)
    {
        DispatcherTimer.RunOnce(
            () =>
            {
                TimeToEndTimeout = null;
                NotifyProperties();
            },
            TimeSpan.FromSeconds(timeLeft));

        DispatcherTimer.Run(() =>
        {
            NotifyProperties();
            return TimeToEndTimeout != null;
        }, TimeSpan.FromSeconds(1));
    }

    public bool CanResendActivation(string email)
    {
        if (IdentityProvider is not PixiAuthIdentityProvider pixiAuthIdentityProvider)
        {
            return false;
        }

        if (email == null || pixiAuthIdentityProvider?.User?.EmailHash == null)
        {
            return false;
        }

        if (pixiAuthIdentityProvider.User?.EmailHash != EmailUtility.GetEmailHash(email)) return true;

        return WaitingForActivation && TimeToEndTimeout == null;
    }

    public async Task<bool> TryValidateSession()
    {
        if (IdentityProvider is not PixiAuthIdentityProvider pixiAuthIdentityProvider)
        {
            return false;
        }

        LastError = null;
        try
        {
            bool validated = await pixiAuthIdentityProvider.TryValidateSession();
            if (validated)
            {
                CurrentEmail = null;
                NotifyProperties();
            }

            return validated;
        }
        catch (Exception ex)
        {
            CrashHelper.SendExceptionInfo(ex);
            return false;
        }
    }

    public async Task Logout()
    {
        if (IdentityProvider is not PixiAuthIdentityProvider pixiAuthIdentityProvider)
        {
            return;
        }

        if (!IsLoggedIn)
        {
            return;
        }

        LastError = null;
        try
        {
            await pixiAuthIdentityProvider.Logout();
        }
        catch (Exception ex)
        {
            CrashHelper.SendExceptionInfo(ex);
        }
    }

    public async Task InstallContent(string productId)
    {
        LastError = null;
        if (IdentityProvider is not PixiAuthIdentityProvider pixiAuthIdentityProvider)
        {
            return;
        }

        if (string.IsNullOrEmpty(productId))
        {
            return;
        }

        try
        {
            string? extensionPath = await AdditionalContentProvider.InstallContent(productId);
            if (extensionPath != null)
            {
                Owner.ExtensionsSubViewModel.LoadExtensionAdHoc(extensionPath);
            }
        }
        catch (Exception ex)
        {
            CrashHelper.SendExceptionInfo(ex);
        }
    }

    private void OnError(string error, object? arg = null)
    {
        if (error == "SESSION_NOT_VALIDATED")
        {
            LastError = null;
        }
        else
        {
            LastError = arg != null ? new LocalizedString(error, arg) : new LocalizedString(error);
        }
    }

    private void NotifyProperties()
    {
        OnPropertyChanged(nameof(User));
        OnPropertyChanged(nameof(NotLoggedIn));
        OnPropertyChanged(nameof(WaitingForActivation));
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(LastError));
        OnPropertyChanged(nameof(TimeToEndTimeout));
        OnPropertyChanged(nameof(TimeToEndTimeoutString));
        OnPropertyChanged(nameof(AvatarUrl));
        OnPropertyChanged(nameof(EmailEqualsLastSentMail));
        OnPropertyChanged(nameof(OwnedProducts));
        ResendActivationCommand.NotifyCanExecuteChanged();
    }
}
