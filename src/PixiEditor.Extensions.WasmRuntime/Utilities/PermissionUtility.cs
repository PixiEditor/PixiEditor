using System.Runtime.CompilerServices;
using PixiEditor.Extensions.Metadata;

namespace PixiEditor.Extensions.WasmRuntime.Utilities;

internal static class PermissionUtility
{
    public static void ThrowIfLacksPermissions(ExtensionMetadata metadata, ExtensionPermissions permissions, [CallerMemberName] string caller = "")
    {
        if (!metadata.Permissions.HasFlag(permissions))
        {
            throw new UnauthorizedAccessException($"Extension '{metadata.UniqueName}' tries to call {caller} but lacks required permissions '{permissions}'.");
        }   
    }
}
