using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Extensions.CommonApi.UserPreferences;

namespace PixiEditor.ViewModels.UserPreferences;

internal class SettingsGroup : ObservableObject
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
}
