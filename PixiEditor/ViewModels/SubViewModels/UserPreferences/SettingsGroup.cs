using System.ComponentModel;
using PixiEditor.Helpers;
using PixiEditor.Models.UserPreferences;

namespace PixiEditor.ViewModels.SubViewModels.UserPreferences
{
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
    }
}