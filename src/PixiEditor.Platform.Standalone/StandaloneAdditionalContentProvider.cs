using System.Security.Principal;
using PixiEditor.IdentityProvider;
using PixiEditor.IdentityProvider.PixiAuth;
using PixiEditor.PixiAuth;
using PixiEditor.PixiAuth.Exceptions;

namespace PixiEditor.Platform.Standalone;

public sealed class StandaloneAdditionalContentProvider : IAdditionalContentProvider
{
    public string ExtensionsPath { get; }
    public PixiAuthIdentityProvider IdentityProvider { get; }

    public event Action<string, object>? OnError;
    public StandaloneAdditionalContentProvider(string extensionsPath, PixiAuthIdentityProvider identityProvider)
    {
        IdentityProvider = identityProvider;
        ExtensionsPath = extensionsPath;
    }

    public async Task<string?> InstallContent(string productId)
    {
        if (!IdentityProvider.ApiValid) return null;

        if (IdentityProvider.User is not { IsLoggedIn: true })
        {
            return null;
        }

        try
        {
            var stream = await IdentityProvider.PixiAuthClient.DownloadProduct(IdentityProvider.User.SessionToken, productId);
            if (stream != null)
            {
                var filePath = Path.Combine(ExtensionsPath, $"{productId}.pixiext");
                await using (var fileStream = File.Create(filePath))
                {
                    await stream.CopyToAsync(fileStream);
                }

                await stream.DisposeAsync();

                return filePath;
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

        return null;
    }

    public bool IsContentOwned(string product)
    {
        if (!PlatformHasContent(product)) return false;

        if (IdentityProvider.User is not { IsLoggedIn: true })
        {
            return false;
        }

        return IdentityProvider.User.OwnedProducts.Contains(product, StringComparer.OrdinalIgnoreCase);
    }

    public bool PlatformHasContent(string product)
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }

    public void Error(string error)
    {
        OnError?.Invoke(error, null);
    }
}
