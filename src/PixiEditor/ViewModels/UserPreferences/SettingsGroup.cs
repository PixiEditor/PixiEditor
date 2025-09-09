using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings;

namespace PixiEditor.ViewModels.UserPreferences;

internal class SettingsGroup : PixiObservableObject
{
    protected static T GetPreference<T>(string name)
    {
        return IPreferences.Current.GetPreference<T>(name);
    }

    protected static T? GetPreference<T>(string name, T? fallbackValue)
    {
        return IPreferences.Current.GetPreference(name, fallbackValue);
    }

    protected void RaiseAndUpdatePreference<T>(string name, T value)
    {
        OnPropertyChanged(name);
        IPreferences.Current.UpdatePreference(name, value);
    }

    protected void RaiseAndUpdatePreference<T>(ref T backingStore, T value, [CallerMemberName] string name = "")
    {
        SetProperty(ref backingStore, value, propertyName: name);
        IPreferences.Current.UpdatePreference(name, value);
    }
    
    protected void RaiseAndUpdatePreference<T>(Setting<T> settingStore, T value, [CallerMemberName] string name = "")
    {
        if (EqualityComparer<T>.Default.Equals(settingStore.Value, value))
            return;

        settingStore.Value = value;
        OnPropertyChanged(name);
    }

    protected void SubscribeValueChanged<T>(Setting<T> settingStore, string propertyName)
    {
        settingStore.ValueChanged += (_, _) => OnPropertyChanged(propertyName);
    }
}
