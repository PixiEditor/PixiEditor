using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Native
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern IntPtr get_installed_extensions();
}
