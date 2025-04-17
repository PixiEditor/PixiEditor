using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes.Commands;
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

    public AsyncRelayCommand<string> RequestLoginCommand { get; }
    public AsyncRelayCommand TryValidateSessionCommand { get; }
    public AsyncRelayCommand ResendActivationCommand { get; }
    public AsyncRelayCommand LogoutCommand { get; }

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

    public UserViewModel(ViewModelMain owner) : base(owner)
    {
        RequestLoginCommand = new AsyncRelayCommand<string>(RequestLogin);
        TryValidateSessionCommand = new AsyncRelayCommand(TryValidateSession);
        ResendActivationCommand = new AsyncRelayCommand(ResendActivation, CanResendActivation);
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
            if (session != null)
            {
                LastError = null;
                User = new User(email) { SessionId = session.Value };
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
    }

    public async Task ResendActivation()
    {
        if (!apiValid) return;

        if (User?.SessionId == null)
        {
            return;
        }

        try
        {
            await PixiAuthClient.ResendActivation(User.Email, User.SessionId.Value);
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

    public bool CanResendActivation()
    {
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
            (string? token, DateTime? expirationDate) =
                await PixiAuthClient.RefreshToken(User.SessionId.Value, User.SessionToken);

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
                await PixiAuthClient.TryClaimSessionToken(User.Email, User.SessionId.Value);
            if (token != null)
            {
                LastError = null;
                User.SessionToken = token;
                User.SessionExpirationDate = expirationDate;
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

        return false;
    }

    public async Task Logout()
    {
        if (!IsLoggedIn)
        {
            return;
        }

        Guid? sessionId = User?.SessionId;
        string? sessionToken = User?.SessionToken;

        User = null;
        NotifyProperties();
        SaveUserInfo();

        if (!apiValid) return;

        try
        {
            await PixiAuthClient.Logout(sessionId.Value, sessionToken);
        }
        catch (PixiAuthException authException)
        {

        }
        catch (HttpRequestException httpRequestException)
        {

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

    private void NotifyProperties()
    {
        OnPropertyChanged(nameof(User));
        OnPropertyChanged(nameof(NotLoggedIn));
        OnPropertyChanged(nameof(WaitingForActivation));
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(LastError));
        OnPropertyChanged(nameof(TimeToEndTimeout));
        OnPropertyChanged(nameof(TimeToEndTimeoutString));
        ResendActivationCommand.NotifyCanExecuteChanged();
    }
}
