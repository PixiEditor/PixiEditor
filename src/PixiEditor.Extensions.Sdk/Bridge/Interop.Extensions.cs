using PixiEditor.Extensions.Sdk.Utilities;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Interop
{
    public static string[] GetInstalledExtensions()
    {
        IntPtr ptr = Native.get_installed_extensions();
        if (ptr == IntPtr.Zero)
        {
            return Array.Empty<string>();
        }

        return InteropUtility.IntPtrToStringArray(ptr);    }
}
