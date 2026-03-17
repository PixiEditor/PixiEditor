using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Native
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern int send_http_request(IntPtr ptr, int length);
}
