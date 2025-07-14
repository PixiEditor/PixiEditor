using PixiEditor.Extensions.Sdk.Utilities;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Interop
{
    public static string[] GetOwnedContent()
    {
        IntPtr ptr = Native.get_owned_content();
        if (ptr == IntPtr.Zero)
        {
            return [];
        }

        return InteropUtility.IntPtrToStringArray(ptr);
    }
}
