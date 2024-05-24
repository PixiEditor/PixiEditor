using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Wasm.Bridge;

internal static partial class Native
{
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void save_preferences();

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void update_preference_int(string name, int value);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void update_preference_bool(string name, bool value);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void update_preference_string(string name, string value);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void update_preference_float(string name, float value);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void update_preference_double(string name, double value);
}
