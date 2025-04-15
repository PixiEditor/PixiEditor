using CommunityToolkit.Mvvm.Input;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.PixiAuth;

namespace PixiEditor.ViewModels.SubViewModels;

internal class UserViewModel : SubViewModel<ViewModelMain>
{
    public PixiAuthClient PixiAuthClient { get; }
    public User? User { get; private set; }

    public bool WaitingForActivation => User is { SessionId: not null } && string.IsNullOrEmpty(User.SessionToken);
    public bool IsLoggedIn => User is { SessionId: not null } && !string.IsNullOrEmpty(User.SessionToken);

    public AsyncRelayCommand<string> RequestLoginCommand { get; }
    public AsyncRelayCommand TryValidateSessionCommand { get; }

    private bool apiValid = true;

    public UserViewModel(ViewModelMain owner) : base(owner)
    {
        RequestLoginCommand = new AsyncRelayCommand<string>(RequestLogin);
        TryValidateSessionCommand = new AsyncRelayCommand(TryValidateSession);

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
    }


    public async Task RequestLogin(string email)
    {
        if (!apiValid) return;

        Guid? session = await PixiAuthClient.GenerateSession(email);
        if (session != null)
        {
            User = new User(email) { SessionId = session.Value };
            OnPropertyChanged(nameof(WaitingForActivation));
            OnPropertyChanged(nameof(IsLoggedIn));
            SaveUserInfo();
        }
    }

    public async Task<bool> TryValidateSession()
    {
        if (!apiValid) return false;

        if (User?.SessionId == null)
        {
            return false;
        }

        string? token = await PixiAuthClient.TryClaimSessionToken(User.Email, User.SessionId.Value);
        if (token != null)
        {
            User.SessionToken = token;
            OnPropertyChanged(nameof(User));
            OnPropertyChanged(nameof(WaitingForActivation));
            OnPropertyChanged(nameof(IsLoggedIn));
            SaveUserInfo();
            return true;
        }

        return false;
    }

    public void SaveUserInfo()
    {
        // TODO:
    }
}
