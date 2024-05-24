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
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void update_local_preference_int(string name, int value);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void update_local_preference_bool(string name, bool value);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void update_local_preference_string(string name, string value);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void update_local_preference_float(string name, float value);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void update_local_preference_double(string name, double value);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern int get_preference_int(string name, int fallbackInt);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern bool get_preference_bool(string name, bool fallbackBool);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern string get_preference_string(string name, string fallbackString);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern float get_preference_float(string name, float fallbackFloat);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern double get_preference_double(string name, double fallbackDouble);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern int get_local_preference_int(string name, int fallbackInt);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern bool get_local_preference_bool(string name, bool fallbackBool);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern string get_local_preference_string(string name, string fallbackString);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern float get_local_preference_float(string name, float fallbackFloat);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern double get_local_preference_double(string name, double fallbackDouble);
}
