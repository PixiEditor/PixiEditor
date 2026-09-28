using PixiEditor.Extensions.CommonApi.Extensions;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.Extensions;

public class ExtensionsProvider : IExtensionsProvider
{
    public string[] GetInstalled()
    {
        return Interop.GetInstalledExtensions();
    }
}
