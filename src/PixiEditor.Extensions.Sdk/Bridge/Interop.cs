using PixiEditor.Extensions.CommonApi.Palettes;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Interop
{
    static Interop()
    {
        uniqueName = Native.get_extension_unique_name();
        Native.PreferenceUpdated += NativeOnPreferenceUpdated;
        Native.CommandInvoked += OnCommandInvoked;
    }

    public static void UpdateUserPreference<T>(string name, T value)
    {
        switch (value)
        {
            case int intValue:
                Native.update_preference_int(name, intValue);
                break;
            case bool boolValue:
                Native.update_preference_bool(name, boolValue);
                break;
            case string stringValue:
                Native.update_preference_string(name, stringValue);
                break;
            case float floatValue:
                Native.update_preference_float(name, floatValue);
                break;
            case double doubleValue:
                Native.update_preference_double(name, doubleValue);
                break;
            default:
                throw new ArgumentException($"Unsupported type {value.GetType().Name}");
        }
    }
    
    public static void UpdateLocalUserPreference<T>(string name, T value)
    {
        switch (value)
        {
            case int intValue:
                Native.update_local_preference_int(name, intValue);
                break;
            case bool boolValue:
                Native.update_local_preference_bool(name, boolValue);
                break;
            case string stringValue:
                Native.update_local_preference_string(name, stringValue);
                break;
            case float floatValue:
                Native.update_local_preference_float(name, floatValue);
                break;
            case double doubleValue:
                Native.update_local_preference_double(name, doubleValue);
                break;
            default:
                throw new ArgumentException($"Unsupported type {value.GetType().Name}");
        }
    }


    public static T GetPreference<T>(string name, T fallbackValue)
    {
        Type type = typeof(T);
        if (type == typeof(int))
        {
            return (T)(object)Native.get_preference_int(name, (int)(object)fallbackValue);
        }
        if (type == typeof(bool))
        {
            return (T)(object)Native.get_preference_bool(name, (bool)(object)fallbackValue);
        }
        if (type == typeof(string))
        {
            return (T)(object)Native.get_preference_string(name, (string)(object)fallbackValue);
        }
        if (type == typeof(float))
        {
            return (T)(object)Native.get_preference_float(name, (float)(object)fallbackValue);
        }
        if (type == typeof(double))
        {
            return (T)(object)Native.get_preference_double(name, (double)(object)fallbackValue);
        }
        
        throw new ArgumentException($"Unsupported type {type.Name}");
    }
    
    public static T GetLocalPreference<T>(string name, T fallbackValue)
    {
        if (typeof(T) == typeof(int))
        {
            return (T)(object)Native.get_local_preference_int(name, (int)(object)fallbackValue);
        }
        if (typeof(T) == typeof(bool))
        {
            return (T)(object)Native.get_local_preference_bool(name, (bool)(object)fallbackValue);
        }
        if (typeof(T) == typeof(string))
        {
            return (T)(object)Native.get_local_preference_string(name, (string)(object)fallbackValue);
        }
        if (typeof(T) == typeof(float))
        {
            return (T)(object)Native.get_local_preference_float(name, (float)(object)fallbackValue);
        }
        if (typeof(T) == typeof(double))
        {
            return (T)(object)Native.get_local_preference_double(name, (double)(object)fallbackValue);
        }
        
        throw new ArgumentException($"Unsupported type {typeof(T).Name}");
    }
}
