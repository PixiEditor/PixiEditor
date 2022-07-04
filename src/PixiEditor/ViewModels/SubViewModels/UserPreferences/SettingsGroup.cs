using System.ComponentModel;
using System.Runtime.CompilerServices;
using PixiEditor.Helpers;
using PixiEditor.Models.UserPreferences;

namespace PixiEditor.ViewModels.SubViewModels.UserPreferences;

public class SettingsGroup : NotifyableObject
{
    protected static T GetPreference<T>(string name)
    {
        return IPreferences.Current.GetPreference<T>(name);
    }

#nullable enable

    protected static T? GetPreference<T>(string name, T? fallbackValue)
    {
        return IPreferences.Current.GetPreference(name, fallbackValue);
    }

#nullable disable

    protected void RaiseAndUpdatePreference<T>(string name, T value)
    {
        RaisePropertyChanged(name);
        IPreferences.Current.UpdatePreference(name, value);
    }

    protected void RaiseAndUpdatePreference<T>(ref T backingStore, T value, [CallerMemberName]string name = "")
    {
        SetProperty(ref backingStore, value, propertyName: name);
        IPreferences.Current.UpdatePreference(name, value);
    }
}