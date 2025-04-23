using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.User;
using PixiEditor.OperatingSystem;
using PixiEditor.PixiAuth;
using PixiEditor.PixiAuth.Exceptions;

namespace PixiEditor.ViewModels.SubViewModels;

internal class UserViewModel : SubViewModel<ViewModelMain>
{
    private LocalizedString? lastError = null;
    public PixiAuthClient PixiAuthClient { get; }
    public User? User { get; private set; }

    public bool NotLoggedIn => User?.SessionId is null || User.SessionId == Guid.Empty;
    public bool WaitingForActivation => User is { SessionId: not null } && string.IsNullOrEmpty(User.SessionToken);
    public bool IsLoggedIn => User is { SessionId: not null } && !string.IsNullOrEmpty(User.SessionToken);
    public bool EmailEqualsLastSentMail => (CurrentEmail != null ? GetEmailHash(CurrentEmail) : "") == lastSentHash;

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

    private bool apiValid = true;

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

    public string? UserGravatarUrl =>
        User?.EmailHash != null ? $"https://www.gravatar.com/avatar/{User.EmailHash}?s=100&d=initials" : null;

    public ObservableCollection<string> OwnedProducts { get; private set; } = new ObservableCollection<string>();

    private string currentEmail = string.Empty;
    private string username;

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

    public string Username
    {
        get => username;
        set
        {
            SetProperty(ref username, value);
        }
    }

    public UserViewModel(ViewModelMain owner) : base(owner)
    {
        RequestLoginCommand = new AsyncRelayCommand<string>(RequestLogin, CanRequestLogin);
        TryValidateSessionCommand = new AsyncRelayCommand(TryValidateSession);
        ResendActivationCommand = new AsyncRelayCommand<string>(ResendActivation, CanResendActivation);
        InstallContentCommand = new AsyncRelayCommand<string>(InstallContent);
        LogoutCommand = new AsyncRelayCommand(Logout);

        string baseUrl = BuildConstants.PixiEditorApiUrl;
#if DEBUG
        if (baseUrl.Contains('{') && baseUrl.Contains('}'))
        {
            string? envUrl = Environment.GetEnvironmentVariable("PIXIEDITOR_API_URL");
            if (envUrl != null)
            {
                baseUrl = envUrl;
            }
        }
#endif
        try
        {
            PixiAuthClient = new PixiAuthClient(baseUrl);
        }
        catch (UriFormatException e)
        {
            Console.WriteLine($"Invalid api URL format: {e.Message}");
            apiValid = false;
        }

        Task.Run(async () =>
        {
            await LoadUserData();
            await TryRefreshToken();
            await LogoutIfTokenExpired();
        });
    }

    public async Task RequestLogin(string email)
    {
        if (!apiValid) return;

        try
        {
            Guid? session = await PixiAuthClient.GenerateSession(email);
            string hash = GetEmailHash(email);
            if (session != null)
            {
                LastError = null;
                Username = null;
                User = new User { SessionId = session.Value, EmailHash = hash, Username = GenerateUsername(hash) };
                lastSentHash = User.EmailHash;
                NotifyProperties();
                SaveUserInfo();
            }
        }
        catch (PixiAuthException authException)
        {
            LastError = new LocalizedString(authException.Message);
        }
        catch (HttpRequestException httpRequestException)
        {
            LastError = new LocalizedString("CONNECTION_ERROR");
        }
        catch (TaskCanceledException timeoutException)
        {
            LastError = new LocalizedString("CONNECTION_TIMEOUT");
        }
    }

    public bool CanRequestLogin(string email)
    {
        return !string.IsNullOrEmpty(email) && email.Contains('@');
    }

    public async Task ResendActivation(string email)
    {
        if (!apiValid) return;

        string emailHash = GetEmailHash(email);
        if (User?.EmailHash != emailHash)
        {
            await RequestLogin(email);
            return;
        }

        if (User?.SessionId == null)
        {
            return;
        }

        try
        {
            await PixiAuthClient.ResendActivation(User.SessionId.Value);
            TimeToEndTimeout = DateTime.Now.Add(TimeSpan.FromSeconds(60));
            RunTimeoutTimers(60);
            NotifyProperties();
            LastError = null;
        }
        catch (TooManyRequestsException e)
        {
            LastError = new LocalizedString(e.Message, e.TimeLeft);
            TimeToEndTimeout = DateTime.Now.Add(TimeSpan.FromSeconds(e.TimeLeft));
            RunTimeoutTimers(e.TimeLeft);
            NotifyProperties();
        }
        catch (PixiAuthException authException)
        {
            LastError = new LocalizedString(authException.Message);
        }
        catch (HttpRequestException httpRequestException)
        {
            LastError = new LocalizedString("CONNECTION_ERROR");
        }
        catch (TaskCanceledException timeoutException)
        {
            LastError = new LocalizedString("CONNECTION_TIMEOUT");
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
        if (email == null || User?.EmailHash == null)
        {
            return false;
        }

        if (User?.EmailHash != GetEmailHash(email)) return true;

        return WaitingForActivation && TimeToEndTimeout == null;
    }

    public async Task<bool> TryRefreshToken()
    {
        if (!apiValid) return false;

        if (!IsLoggedIn)
        {
            return false;
        }

        try
        {
            (string? token, DateTime? expirationDate) = await PixiAuthClient.RefreshToken(User.SessionToken);

            if (token != null)
            {
                User.SessionToken = token;
                User.SessionExpirationDate = expirationDate;
                NotifyProperties();
                SaveUserInfo();
                return true;
            }
        }
        catch (ForbiddenException e)
        {
            User = null;
            NotifyProperties();
            SaveUserInfo();
            LastError = new LocalizedString(e.Message);
        }
        catch (PixiAuthException authException)
        {
            LastError = new LocalizedString(authException.Message);
        }
        catch (HttpRequestException httpRequestException)
        {
            LastError = new LocalizedString("CONNECTION_ERROR");
        }
        catch (TaskCanceledException timeoutException)
        {
            LastError = new LocalizedString("CONNECTION_TIMEOUT");
        }

        return false;
    }

    public async Task<bool> TryValidateSession()
    {
        if (!apiValid) return false;

        if (User?.SessionId == null)
        {
            return false;
        }

        try
        {
            (string? token, DateTime? expirationDate) =
                await PixiAuthClient.TryClaimSessionToken(User.SessionId.Value);
            if (token != null)
            {
                LastError = null;
                User.SessionToken = token;
                User.SessionExpirationDate = expirationDate;
                var products = await PixiAuthClient.GetOwnedProducts(User.SessionToken);
                if (products != null)
                {
                    User.OwnedProducts = products;
                    OwnedProducts = new ObservableCollection<string>(User.OwnedProducts);
                    NotifyProperties();
                }

                Task.Run(async () =>
                {
                    string username = User.Username;
                    User.Username = await TryFetchUserName(User.EmailHash);
                    Username = User.Username;
                    if (username != User.Username)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            NotifyProperties();
                            SaveUserInfo();
                        });
                    }
                });

                CurrentEmail = null;
                NotifyProperties();
                SaveUserInfo();
                return true;
            }
        }
        catch (BadRequestException ex)
        {
            if (ex.Message == "SESSION_NOT_VALIDATED")
            {
                LastError = null;
            }
        }
        catch (PixiAuthException authException)
        {
            LastError = new LocalizedString(authException.Message);
        }
        catch (HttpRequestException httpRequestException)
        {
            LastError = new LocalizedString("CONNECTION_ERROR");
        }
        catch (TaskCanceledException timeoutException)
        {
            LastError = new LocalizedString("CONNECTION_TIMEOUT");
        }

        return false;
    }

    public async Task Logout()
    {
        if (!IsLoggedIn)
        {
            return;
        }

        string? sessionToken = User?.SessionToken;

        User = null;
        LastError = null;
        OwnedProducts.Clear();
        Username = string.Empty;
        NotifyProperties();
        SaveUserInfo();

        if (!apiValid) return;

        try
        {
            await PixiAuthClient.Logout(sessionToken);
        }
        catch (PixiAuthException authException)
        {
        }
        catch (HttpRequestException httpRequestException)
        {
        }
        catch (TaskCanceledException timeoutException)
        {
        }
    }

    public async Task InstallContent(string productId)
    {
        if (!apiValid) return;

        if (User?.SessionToken == null)
        {
            LastError = new LocalizedString("NOT_LOGGED_IN");
            return;
        }

        try
        {
            var stream = await PixiAuthClient.DownloadProduct(User.SessionToken, productId);
            if (stream != null)
            {
                var packagesPath = Owner.ExtensionsSubViewModel.ExtensionLoader.PackagesPath;
                var filePath = Path.Combine(packagesPath, $"{productId}.pixiext");
                await using (var fileStream = File.Create(filePath))
                {
                    await stream.CopyToAsync(fileStream);
                }

                await stream.DisposeAsync();

                Owner.ExtensionsSubViewModel.LoadExtensionAdHoc(filePath);
                LastError = null;
            }
        }
        catch (PixiAuthException authException)
        {
            LastError = new LocalizedString(authException.Message);
        }
        catch (HttpRequestException httpRequestException)
        {
            LastError = new LocalizedString("CONNECTION_ERROR");
        }
        catch (TaskCanceledException timeoutException)
        {
            LastError = new LocalizedString("CONNECTION_TIMEOUT");
        }
    }

    public async Task SaveUserInfo()
    {
        try
        {
            await SecureStorage.SetValueAsync("UserData", User);
        }
        catch (Exception e)
        {
            CrashHelper.SendExceptionInfo(e);
        }
    }

    public async Task LoadUserData()
    {
        try
        {
            User = await SecureStorage.GetValueAsync<User>("UserData", null);
            try
            {
                User.Username = await TryFetchUserName(User.EmailHash);
                var products = await PixiAuthClient.GetOwnedProducts(User.SessionToken);
                if (products != null)
                {
                    User.OwnedProducts = products;
                    OwnedProducts = new ObservableCollection<string>(User.OwnedProducts);
                }

                Username = User.Username;
                NotifyProperties();
            }
            catch
            {
            }
        }
        catch (Exception e)
        {
            CrashHelper.SendExceptionInfo(e);
            User = null;
            NotifyProperties();
            LastError = "FAIL_LOAD_USER_DATA";
        }
    }

    public async Task LogoutIfTokenExpired()
    {
        if (User?.SessionExpirationDate != null && User.SessionExpirationDate < DateTime.Now)
        {
            await Logout();
            LastError = new LocalizedString("SESSION_EXPIRED");
        }
    }

    private async Task<string> TryFetchUserName(string emailHash)
    {
        try
        {
            string? username = await Gravatar.GetUsername(emailHash);
            if (username != null)
            {
                return username;
            }
        }
        catch (PixiAuthException authException)
        {
            LastError = new LocalizedString(authException.Message);
        }
        catch (HttpRequestException httpRequestException)
        {
            LastError = new LocalizedString("CONNECTION_ERROR");
        }
        catch (TaskCanceledException timeoutException)
        {
            LastError = new LocalizedString("CONNECTION_TIMEOUT");
        }

        return GenerateUsername(emailHash);
    }

    private string GetEmailHash(string email)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(email.ToLower());
        byte[] hashBytes = sha256.ComputeHash(inputBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private string GenerateUsername(string emailHash)
    {
        return UsernameGenerator.GenerateUsername(emailHash);
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
        OnPropertyChanged(nameof(UserGravatarUrl));
        OnPropertyChanged(nameof(EmailEqualsLastSentMail));
        OnPropertyChanged(nameof(OwnedProducts));
        ResendActivationCommand.NotifyCanExecuteChanged();
    }
}
