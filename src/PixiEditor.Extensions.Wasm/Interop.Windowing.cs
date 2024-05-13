using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Wasm;

internal static partial class Interop
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int CreatePopupWindow(string title, IntPtr data, int length);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void SetWindowTitle(int windowHandle, string title);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern string GetWindowTitle(int windowHandle);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void ShowWindow(int windowHandle);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void CloseWindow(int windowHandle);
}
