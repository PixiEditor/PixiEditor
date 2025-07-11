using PixiEditor.Extensions.Metadata;
using PixiEditor.Extensions.WasmRuntime.Utilities;
using static System.Array;

namespace PixiEditor.Extensions.WasmRuntime.Api;

internal class UserApi : ApiGroupHandler
{
    [ApiFunction("is_user_logged_in")]
    public bool IsLoggedIn()
    {
        return Api.UserDataProvider?.IsLoggedIn ?? false;
    }

    [ApiFunction("get_username")]
    public string GetUsername()
    {
        PermissionUtility.ThrowIfLacksPermissions(Metadata, ExtensionPermissions.ReadUserData);
        return Api.UserDataProvider?.Username ?? string.Empty;
    }

    [ApiFunction("get_account_provider_name")]
    public string GetAccountProviderName()
    {
        return Api.UserDataProvider?.AccountProviderName ?? string.Empty;
    }

    [ApiFunction("get_owned_content")]
    public byte[] GetOwnedContent()
    {
        PermissionUtility.ThrowIfLacksPermissions(Metadata, ExtensionPermissions.ReadUserData);
        string[] content = Api.UserDataProvider.GetOwnedContent();

        byte[] bytes = InteropUtility.SerializeToBytes(content);
        return bytes;
    }
}
