using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Wasm.Bridge;

internal static partial class Native
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int create_popup_window(string title, IntPtr data, int length);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void set_window_title(int windowHandle, string title);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern string get_window_title(int windowHandle);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double get_window_width(int windowHandle);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void set_window_width(int windowHandle, double width);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double get_window_height(int windowHandle);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void set_window_height(int windowHandle, double height);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern bool get_window_resizable(int windowHandle);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void set_window_resizable(int windowHandle, bool resizable);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern bool get_window_minimizable(int windowHandle);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void set_window_minimizable(int windowHandle, bool minimizable);
    

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void show_window(int windowHandle);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void close_window(int windowHandle);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int show_window_async(int windowHandle);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern int get_built_in_window(int type);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern int get_window(string windowId);
}
