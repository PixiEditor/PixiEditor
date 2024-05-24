using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.Wasm.Bridge;

namespace PixiEditor.Extensions.Wasm.Api.UserPreferences;

/// <summary>
///     Add, remove and update user preferences.
/// </summary>
public class Preferences : IPreferences
{
    public void Save()
    {
        Native.save_preferences();
    }
    
    public void UpdatePreference<T>(string name, T value)
    {
        Interop.UpdateUserPreference(name, value);
    }

    public void UpdateLocalPreference<T>(string name, T value)
    {
        Interop.UpdateLocalUserPreference(name, value);
    }

    public T GetPreference<T>(string name)
    {
        return Interop.GetPreference<T>(name, default);
    }

    public T GetPreference<T>(string name, T fallbackValue)
    {
        return Interop.GetPreference<T>(name, fallbackValue);
    }

    public T GetLocalPreference<T>(string name)
    {
        return Interop.GetLocalPreference<T>(name, default);
    }

    public T GetLocalPreference<T>(string name, T fallbackValue)
    {
        return Interop.GetLocalPreference(name, fallbackValue);
    }


    public void AddCallback(string name, Action<object> action)
    {
        
    }

    public void AddCallback<T>(string name, Action<T> action)
    {
        throw new NotImplementedException();
    }

    public void RemoveCallback(string name, Action<object> action)
    {
        throw new NotImplementedException();
    }

    public void RemoveCallback<T>(string name, Action<T> action)
    {
        throw new NotImplementedException();
    }

    void IPreferences.Init() { }

    void IPreferences.Init(string path, string localPath) { }
}
