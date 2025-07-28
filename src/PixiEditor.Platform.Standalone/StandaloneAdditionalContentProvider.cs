using System.Reflection;
using System.Security.Principal;
using PixiEditor.IdentityProvider;
using PixiEditor.IdentityProvider.PixiAuth;
using PixiEditor.PixiAuth;
using PixiEditor.PixiAuth.Exceptions;

namespace PixiEditor.Platform.Standalone;

public sealed class StandaloneAdditionalContentProvider : IAdditionalContentProvider
{
    public string[] ExtensionsPaths { get; }
    public PixiAuthIdentityProvider IdentityProvider { get; }

    public event Action<string, object>? OnError;

    public StandaloneAdditionalContentProvider(string[] extensionsPaths, PixiAuthIdentityProvider identityProvider)
    {
        IdentityProvider = identityProvider;
        ExtensionsPaths = extensionsPaths;
    }

    public async Task<string?> InstallContent(string productId)
    {
        if (!IdentityProvider.IsValid || ExtensionsPaths == null || ExtensionsPaths.Length == 0) return null;

        if (IdentityProvider.User is not { IsLoggedIn: true })
        {
            return null;
        }

        try
        {
            var stream =
                await IdentityProvider.PixiAuthClient.DownloadProduct(IdentityProvider.User.SessionToken, productId);
            if (stream != null)
            {
                var firstExistingPath =
                    ExtensionsPaths.FirstOrDefault(path => File.Exists(Path.Combine(path, $"{productId}.pixiext")));
                if (firstExistingPath != null)
                {
                    var updatePath = Path.Combine(firstExistingPath, $"{productId}.update");
                    await using (var fileStream = File.Create(updatePath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    await stream.DisposeAsync();
                    return updatePath;
                }

                var filePath = Path.Combine(ExtensionsPaths[0], $"{productId}.pixiext");
                try
                {
                    await using (var fileStream = File.Create(filePath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    await stream.DisposeAsync();
                }
                catch (IOException e)
                {
                    filePath = Path.Combine(ExtensionsPaths[0], $"{productId}.update");
                    await using (var fileStream = File.Create(filePath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    await stream.DisposeAsync();
                    return null;
                }

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

    public bool IsInstalled(string productId)
    {
        if (string.IsNullOrEmpty(productId)) return false;

        var firstExistingPath =
            ExtensionsPaths.FirstOrDefault(path => File.Exists(Path.Combine(path, $"{productId}.pixiext")));
        return firstExistingPath != null;
    }

    public bool IsContentOwned(string product)
    {
        if (!PlatformHasContent(product)) return false;

        if (IdentityProvider.User is not { IsLoggedIn: true })
        {
            return false;
        }

        return IdentityProvider.User.OwnedProducts.Any(x => x.Id.Equals(product, StringComparison.OrdinalIgnoreCase));
    }

    public bool PlatformHasContent(string product)
    {
        return true;
    }

    public void Error(string error)
    {
        OnError?.Invoke(error, null);
    }
}
