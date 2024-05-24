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
}
