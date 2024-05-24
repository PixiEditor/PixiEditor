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
        throw new NotImplementedException();
    }

    public T GetPreference<T>(string name)
    {
        throw new NotImplementedException();
    }

    public T GetPreference<T>(string name, T fallbackValue)
    {
        throw new NotImplementedException();
    }

    public T GetLocalPreference<T>(string name)
    {
        throw new NotImplementedException();
    }

    public T GetLocalPreference<T>(string name, T fallbackValue)
    {
        throw new NotImplementedException();
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
