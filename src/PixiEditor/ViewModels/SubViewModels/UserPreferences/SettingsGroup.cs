using System.Runtime.CompilerServices;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Helpers;

namespace PixiEditor.ViewModels.SubViewModels.UserPreferences;

internal class SettingsGroup : NotifyableObject
{
    protected static T GetPreference<T>([RemotePreferenceConstant] string name)
    {
        return IPreferences.Current.GetPreference<T>(name);
    }

#nullable enable

    protected static T? GetPreference<T>([RemotePreferenceConstant] string name, T? fallbackValue)
    {
        return IPreferences.Current.GetPreference(name, fallbackValue);
    }

#nullable disable

    protected void RaiseAndUpdatePreference<T>([RemotePreferenceConstant] string name, T value)
    {
        RaisePropertyChanged(name);
        IPreferences.Current.UpdatePreference(name, value);
    }

    protected void RaiseAndUpdatePreference<T>(ref T backingStore, T value, [CallerMemberName, RemotePreferenceConstant] string name = "")
    {
        SetProperty(ref backingStore, value, propertyName: name);
        IPreferences.Current.UpdatePreference(name, value);
    }
}
