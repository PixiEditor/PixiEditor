namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Interop
{
    private static Dictionary<string, List<Action<string, object>>> _callbacks = new();
    private static string uniqueName;

    private static void NativeOnPreferenceUpdated(string name, object value)
    {
        if (_callbacks.TryGetValue(name, out var actions))
        {
            foreach (var action in actions)
            {
                action(name, value);
            }
        }
        else if (_callbacks.TryGetValue(name.Replace($"{uniqueName}:", ""), out var uniqueActions))
        {
            foreach (var action in uniqueActions)
            {
                action(name, value);
            }
        }
    }

    public static void AddPreferenceCallback(string name, Action<string, object> action)
    {
        if (_callbacks.TryAdd(name, new List<Action<string, object>>()))
        {
            Native.add_preference_callback(name);
        }
        
        _callbacks[name].Add(action);
    }
    
    public static void AddPreferenceCallback<T>(string name, Action<string, T> action)
    {
        AddPreferenceCallback(name, (preferenceName, value) => action(preferenceName, (T)value));
    }

    public static void RemovePreferenceCallback(string name, Action<string, object> action)
    {
        if (_callbacks.TryGetValue(name, out var actions))
        {
            actions.Remove(action);
            if (actions.Count == 0)
            {
                _callbacks.Remove(name);
                Native.remove_preference_callback(name);
            }
        }
    }
    
    public static void RemovePreferenceCallback<T>(string name, Action<string, T> action)
    {
        RemovePreferenceCallback(name, (prefName, value) => action(prefName, (T)value));
    }
}
