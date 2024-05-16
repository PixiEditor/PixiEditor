using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PixiEditor.Extensions.Wasm;

internal static partial class Interop
{
    [DllImport("interop")]
    internal static extern int create_popup_window(string title, IntPtr data, int length);

    [DllImport("interop")]
    internal static extern void set_window_title(int windowHandle, string title);

    [DllImport("interop")]
    internal static extern string get_window_title(int windowHandle);

    [DllImport("interop")]
    internal static extern void show_window(int windowHandle);

    [DllImport("interop")]
    internal static extern void close_window(int windowHandle);
}
