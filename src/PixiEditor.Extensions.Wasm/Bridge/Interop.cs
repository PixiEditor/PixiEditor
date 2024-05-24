namespace PixiEditor.Extensions.Wasm.Bridge;

internal static class Interop
{
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
        return fallbackValue switch
        {
            int intFallback => (T)(object)Native.get_preference_int(name, intFallback),
            bool boolFallback => (T)(object)Native.get_preference_bool(name, boolFallback),
            string stringFallback => (T)(object)Native.get_preference_string(name, stringFallback),
            float floatFallback => (T)(object)Native.get_preference_float(name, floatFallback),
            double doubleFallback => (T)(object)Native.get_preference_double(name, doubleFallback),
            _ => throw new ArgumentException($"Unsupported type {fallbackValue.GetType().Name}")
        };
    }
    
    public static T GetLocalPreference<T>(string name, T fallbackValue)
    {
        return fallbackValue switch
        {
            int intFallback => (T)(object)Native.get_local_preference_int(name, intFallback),
            bool boolFallback => (T)(object)Native.get_local_preference_bool(name, boolFallback),
            string stringFallback => (T)(object)Native.get_local_preference_string(name, stringFallback),
            float floatFallback => (T)(object)Native.get_local_preference_float(name, floatFallback),
            double doubleFallback => (T)(object)Native.get_local_preference_double(name, doubleFallback),
            _ => throw new ArgumentException($"Unsupported type {fallbackValue.GetType().Name}")
        };
    }
}
