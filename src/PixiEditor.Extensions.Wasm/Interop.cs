using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PixiEditor.Extensions.Wasm;

internal class Interop
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void LogMessage(string message);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void CreatePopupWindow(string title, string body);
}
