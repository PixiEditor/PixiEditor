using PixiEditor.OperatingSystem;
using PixiEditor.PixiAuth;
using PixiEditor.PixiAuth.Exceptions;
using PixiEditor.PixiAuth.Utils;

namespace PixiEditor.IdentityProvider.PixiAuth;

public class PixiAuthIdentityProvider : IIdentityProvider
{
    public string ProviderName { get; } = "PixiAuth";
    public bool AllowsLogout { get; } = true;
    public bool ApiValid => apiValid;
    private bool apiValid = true;
    public PixiAuthClient PixiAuthClient { get; }
    public PixiUser User { get; private set; }
    public bool IsLoggedIn => User?.IsLoggedIn ?? false;
    public Uri? EditProfileUrl { get; } = new Uri("https://gravatar.com/connect");

    public event Action<string, object>? OnError;
    public event Action<List<ProductData>>? OwnedProductsUpdated;
    public event Action<string>? UsernameUpdated;
    public event Action<PixiUser>? LoginRequestSuccessful;
    public event Action<double>? LoginTimeout;
    public event Action? LoggedOut;

    IUser IIdentityProvider.User => User;

    public PixiAuthIdentityProvider(string pixiEditorApiUrl)
    {
        try
        {
            PixiAuthClient = new PixiAuthClient(pixiEditorApiUrl);
        }
        catch (UriFormatException e)
        {
            Console.WriteLine($"Invalid api URL format: {e.Message}");
            apiValid = false;
        }
    }

    public void Initialize()
    {
        User = SecureStorage.GetValue<PixiUser>("UserData", null);
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
            string hash = EmailUtility.GetEmailHash(email);
            if (session != null)
            {
                User = new PixiUser()
                {
                    SessionId = session.Value, EmailHash = hash, Username = GenerateUsername(hash)
                };

                LoginRequestSuccessful?.Invoke(User);

                SaveUserInfo();
            }
        }
        catch (PixiAuthException authException)
        {
            Error(authException.Message);
        }
        catch (HttpRequestException httpRequestException)
        {
            Error("CONNECTION_ERROR");
        }
        catch (TaskCanceledException timeoutException)
        {
            Error("CONNECTION_TIMEOUT");
        }
    }

    public async Task ResendActivation(string email)
    {
        if (!apiValid) return;

        string emailHash = EmailUtility.GetEmailHash(email);
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
            LoginTimeout?.Invoke(60);
        }
        catch (TooManyRequestsException e)
        {
            Error(e.Message, e.TimeLeft);
            LoginTimeout?.Invoke(e.TimeLeft);
        }
        catch (PixiAuthException authException)
        {
            Error(authException.Message);
        }
        catch (HttpRequestException httpRequestException)
        {
            Error("CONNECTION_ERROR");
        }
        catch (TaskCanceledException timeoutException)
        {
            Error("CONNECTION_TIMEOUT");
        }
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
                SaveUserInfo();
                return true;
            }
        }
        catch (ForbiddenException e)
        {
            User = null;
            LoggedOut?.Invoke();
            SaveUserInfo();
            Error(e.Message);
        }
        catch (PixiAuthException authException)
        {
            Error(authException.Message);
        }
        catch (HttpRequestException httpRequestException)
        {
            Error("CONNECTION_ERROR");
        }
        catch (TaskCanceledException timeoutException)
        {
            Error("CONNECTION_TIMEOUT");
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
        LoggedOut?.Invoke();
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

    public async Task SaveUserInfo()
    {
        await SecureStorage.SetValueAsync("UserData", User);
    }

    public async Task LoadUserData()
    {
        try
        {
            User.Username = await TryFetchUserName(User.EmailHash);
            UsernameUpdated?.Invoke(User.Username);
            var products = await PixiAuthClient.GetOwnedProducts(User.SessionToken);
            if (products != null)
            {
                User.OwnedProducts = products.Where(x => x is { IsDlc: true, Target: "PixiEditor" })
                    .Select(x => new ProductData(x.ProductId, x.ProductName)).ToList();
                OwnedProductsUpdated?.Invoke(new List<ProductData>(User.OwnedProducts));
            }
        }
        catch (Exception e)
        {
            Error("FAIL_LOAD_USER_DATA");
        }
    }

    public async Task LogoutIfTokenExpired()
    {
        if (User?.SessionExpirationDate != null && User.SessionExpirationDate < DateTime.Now)
        {
            await Logout();
            Error("SESSION_EXPIRED");
        }
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
                User.SessionToken = token;
                User.SessionExpirationDate = expirationDate;
                var products = await PixiAuthClient.GetOwnedProducts(User.SessionToken);
                if (products != null)
                {
                    User.OwnedProducts = products.Where(x => x.IsDlc && x.Target == "PixiEditor")
                        .Select(x => new ProductData(x.ProductId, x.ProductName)).ToList();
                    OwnedProductsUpdated?.Invoke(new List<ProductData>(User.OwnedProducts));
                }

                Task.Run(async () =>
                {
                    string username = User.Username;
                    User.Username = await TryFetchUserName(User.EmailHash);
                    if (username != User.Username)
                    {
                        UsernameUpdated?.Invoke(User.Username);
                        SaveUserInfo();
                    }
                });

                SaveUserInfo();
                return true;
            }
        }
        catch (BadRequestException ex)
        {
            Error(ex.Message);
        }
        catch (PixiAuthException authException)
        {
            Error(authException.Message);
        }
        catch (HttpRequestException httpRequestException)
        {
            Error("CONNECTION_ERROR");
        }
        catch (TaskCanceledException timeoutException)
        {
            Error("CONNECTION_TIMEOUT");
        }

        return false;
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
            Error(authException.Message);
        }
        catch (HttpRequestException httpRequestException)
        {
            Error("CONNECTION_ERROR");
        }
        catch (TaskCanceledException timeoutException)
        {
            Error("CONNECTION_TIMEOUT");
        }

        return GenerateUsername(emailHash);
    }

    private string GenerateUsername(string emailHash)
    {
        return UsernameGenerator.GenerateUsername(emailHash);
    }

    private void Error(string exception, object? arg = null) => OnError?.Invoke(exception, arg);
}
