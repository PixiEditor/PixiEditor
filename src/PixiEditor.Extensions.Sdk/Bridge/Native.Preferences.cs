using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Native
{
    public static event Action<string, object> PreferenceUpdated;
    
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
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void add_preference_callback(string name);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void remove_preference_callback(string name);
    
    [ApiExport("string_preference_updated")]
    public static void OnStringPreferenceUpdated(string name, string value)
    {
        PreferenceUpdated?.Invoke(name, value);
    }
    
    [ApiExport("int32_preference_updated")]
    public static void OnIntPreferenceUpdated(string name, int value)
    {
        PreferenceUpdated?.Invoke(name, value);
    }
    
    [ApiExport("bool_preference_updated")]
    public static void OnBoolPreferenceUpdated(string name, bool value)
    {
        PreferenceUpdated?.Invoke(name, value);
    }
    
    [ApiExport("float_preference_updated")]
    public static void OnFloatPreferenceUpdated(string name, float value)
    {
        PreferenceUpdated?.Invoke(name, value);
    }
    
    [ApiExport("double_preference_updated")]
    public static void OnDoublePreferenceUpdated(string name, double value)
    {
        PreferenceUpdated?.Invoke(name, value);
    }
    
    [ApiExport("byte_array_preference_updated")]
    public static void OnByteArrayPreferenceUpdated(string name, IntPtr ptr, int length)
    {
        var bytes = new byte[length];
        Marshal.Copy(ptr, bytes, 0, length);
        PreferenceUpdated?.Invoke(name, bytes);
    }
}
