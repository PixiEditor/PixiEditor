using System;
using PixiEditor.Models.UserPreferences;

namespace PixiEditorTests.Mocks;

public class PreferenceSettingsMock : IPreferences
{
    public void AddCallback(string setting, Action<object> action)
    {
    }

    public void AddCallback<T>(string name, Action<T> action)
    {
    }

#nullable enable

    public T? GetLocalPreference<T>(string name)
    {
        return default;
    }

    public T? GetLocalPreference<T>(string name, T? fallbackValue)
    {
        return fallbackValue;
    }

    public T? GetPreference<T>(string name)
    {
        return default;
    }

    public T? GetPreference<T>(string name, T? fallbackValue)
    {
        return fallbackValue;
    }

#nullable disable

    public void Init()
    {
    }

    public void Init(string path, string localPath)
    {
    }

    public void Save()
    {
    }

    public void UpdateLocalPreference<T>(string name, T value)
    {
    }

    public void UpdatePreference<T>(string name, T value)
    {
    }
}