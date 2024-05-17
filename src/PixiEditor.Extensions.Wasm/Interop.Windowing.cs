using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PixiEditor.Extensions.Wasm;

internal static partial class Interop
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int create_popup_window(string title, IntPtr data, int length);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void set_window_title(int windowHandle, string title);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern string get_window_title(int windowHandle);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void show_window(int windowHandle);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void close_window(int windowHandle);
}
