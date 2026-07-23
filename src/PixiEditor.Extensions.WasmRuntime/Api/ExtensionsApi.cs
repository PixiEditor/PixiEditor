using PixiEditor.Extensions.WasmRuntime.Utilities;

namespace PixiEditor.Extensions.WasmRuntime.Api;

internal class ExtensionsApi : ApiGroupHandler
{
    [ApiFunction("get_installed_extensions")]
    public byte[] GetInstalledExtensions()
    {
        string[] installed = Api.Extensions.GetInstalled();

        byte[] bytes = InteropUtility.SerializeToBytes(installed);
        return bytes;
    }
}
