using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Helpers;
using PixiEditor.IdentityProvider;
using PixiEditor.IdentityProvider.PixiAuth;
using PixiEditor.PixiAuth.Utils;
using PixiEditor.Platform;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.User;

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
            if (TimeToEndTimeout == null || !EmailEqualsLastSentMail)
            {
                return string.Empty;
            }

            TimeSpan timeLeft = TimeToEndTimeout.Value - DateTime.Now;
            if(timeLeft.TotalHours > 1)
                return $"({timeLeft:hh\\:mm\\:ss})";
            if(timeLeft.TotalMinutes > 1)
                return $"({timeLeft:mm\\:ss})";

            return timeLeft.TotalSeconds > 0 ? $"({timeLeft:ss})" : string.Empty;
        }
    }

    public ObservableCollection<OwnedProductViewModel> OwnedProducts { get; } =
        new ObservableCollection<OwnedProductViewModel>();

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

    public string? AvatarUrl => IdentityProvider?.User?.AvatarUrl;

    public bool NonDefaultIdentityProvider => IdentityProvider is not PixiAuthIdentityProvider;
    public bool AnyUpdateAvailable => OwnedProducts.Any(x => x.UpdateAvailable);

    private IDisposable? timerCancelable;

    public static string FoundersBundleLink =>
#if STEAM
        "https://store.steampowered.com/app/2435860/PixiEditor__Supporter_Pack/";
#else
        "https://pixieditor.net/download/";
#endif

    public UserViewModel(ViewModelMain owner) : base(owner)
    {
        IdentityProvider = IPlatform.Current?.IdentityProvider;
        AdditionalContentProvider = IPlatform.Current?.AdditionalContentProvider;
        RequestLoginCommand = new AsyncRelayCommand<string>(RequestLogin, CanRequestLogin);
        TryValidateSessionCommand = new AsyncRelayCommand(TryValidateSession);
        ResendActivationCommand = new AsyncRelayCommand<string>(ResendActivation, CanResendActivation);
        InstallContentCommand = new AsyncRelayCommand<string>(InstallContent, CanInstallContent);
        LogoutCommand = new AsyncRelayCommand(Logout);

        if (IdentityProvider == null) return;

        IdentityProvider.OnError += OnError;
        IdentityProvider.OwnedProductsUpdated += IdentityProviderOnOwnedProductsUpdated;
        IdentityProvider.UsernameUpdated += IdentityProviderOnUsernameUpdated;

        if (IdentityProvider is PixiAuthIdentityProvider pixiAuth)
        {
            pixiAuth.LoginRequestSuccessful += PixiAuthOnLoginRequestSuccessful;
            pixiAuth.LoginTimeout += PixiAuthOnLoginTimeout;
            pixiAuth.LoggedOut += PixiAuthOnLoggedOut;
        }

        if (IdentityProvider?.User != null)
        {
            IdentityProviderOnOwnedProductsUpdated(IdentityProvider.User.OwnedProducts);
        }
    }

    private void IdentityProviderOnUsernameUpdated(string newUsername)
    {
        NotifyProperties();
    }

    private void IdentityProviderOnOwnedProductsUpdated(List<ProductData> products)
    {
        OwnedProducts.Clear();
        if (products == null)
        {
            return;
        }
        
        
        foreach (ProductData product in products)
        {
            bool isInstalled = IsInstalled(product.Id);

            string? installedVersion = null;
            if (isInstalled)
            {
                installedVersion = Owner.ExtensionsSubViewModel.ExtensionLoader.LoadedExtensions
                    .FirstOrDefault(x => x.Metadata.UniqueName == product.Id)?.Metadata.Version;
            }
            else
            {
                bool productDownloadedAtLeastOnce = IPreferences.Current.GetLocalPreference<bool>(
                    $"product_{product.Id}_downloaded_at_least_once", false);
                if (!productDownloadedAtLeastOnce)
                {
                    Dispatcher.UIThread.InvokeAsync(async () => await InstallContent(product.Id));
                    IPreferences.Current.UpdateLocalPreference($"product_{product.Id}_downloaded_at_least_once", true);
                }
            }

            OwnedProducts.Add(new OwnedProductViewModel(product, isInstalled, installedVersion, InstallContentCommand,
                IsInstalled));
        }

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
                lastSentHash = EmailUtility.GetEmailHash(email);
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
        return IdentityProvider is PixiAuthIdentityProvider && !string.IsNullOrEmpty(email) && email.Contains('@') &&
               !(HasTimeout() && EmailEqualsLastSentMail);
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

    public bool HasTimeout()
    {
        if (TimeToEndTimeout != null)
        {
            return DateTime.Now < TimeToEndTimeout;
        }

        return false;
    }

    private void RunTimeoutTimers(double timeLeft)
    {
        DispatcherTimer.RunOnce(
            () =>
            {
                if (TimeToEndTimeout.HasValue && TimeToEndTimeout.Value > DateTime.Now)
                {
                    return;
                }
                TimeToEndTimeout = null;
                LastError = null;
                NotifyProperties();
            },
            TimeSpan.FromSeconds(timeLeft));

        timerCancelable?.Dispose();
        timerCancelable = DispatcherTimer.Run(
            () =>
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

    public bool CanInstallContent(string productId)
    {
        return !IsInstalled(productId) || UpdateAvailable(productId);
    }

    private bool UpdateAvailable(string productId)
    {
        ProductData product = IdentityProvider.User.OwnedProducts
            .FirstOrDefault(x => x.Id == productId);

        if (product == null)
        {
            return false;
        }

        return Owner.ExtensionsSubViewModel.ExtensionLoader.LoadedExtensions
            .FirstOrDefault(x => x.Metadata.UniqueName == productId)?.Metadata.Version != product.LatestVersion;
    }

    private bool IsInstalled(string productId)
    {
        if (AdditionalContentProvider.IsInstalled(productId))
        {
            return true;
        }

        return Owner.ExtensionsSubViewModel.ExtensionLoader.LoadedExtensions.Any(x =>
            x.Metadata.UniqueName == productId);
    }

    public async Task InstallContent(string productId)
    {
        LastError = null;

        if (string.IsNullOrEmpty(productId))
        {
            return;
        }

        await Owner.ExtensionsSubViewModel.InstallAndLoadExtension(AdditionalContentProvider,  productId);
    }

    private void OnError(string error, object? arg = null)
    {
        if (error != "TOO_MANY_REQUESTS")
        {
            TimeToEndTimeout = null;
            timerCancelable?.Dispose();
            timerCancelable = null;
            NotifyProperties();
        }
        if (error == "SESSION_NOT_VALIDATED")
        {
            LastError = null;
        }
        else
        {
            LastError = arg != null ? new LocalizedString(error, arg) : new LocalizedString(error);
            if (User is PixiUser { IsWaitingForActivation: true } pixiUser)
            {
                pixiUser.SessionId = null;
                pixiUser.SessionToken = null;
                pixiUser.SessionExpirationDate = null;
                NotifyProperties();
            }
        }
    }

    private void NotifyProperties()
    {
        OnPropertyChanged(nameof(User));
        OnPropertyChanged(nameof(Username));
        OnPropertyChanged(nameof(NotLoggedIn));
        OnPropertyChanged(nameof(WaitingForActivation));
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(LastError));
        OnPropertyChanged(nameof(TimeToEndTimeout));
        OnPropertyChanged(nameof(TimeToEndTimeoutString));
        OnPropertyChanged(nameof(AvatarUrl));
        OnPropertyChanged(nameof(EmailEqualsLastSentMail));
        OnPropertyChanged(nameof(OwnedProducts));
        OnPropertyChanged(nameof(AnyUpdateAvailable));
        ResendActivationCommand.NotifyCanExecuteChanged();
        RequestLoginCommand.NotifyCanExecuteChanged();
    }
}
