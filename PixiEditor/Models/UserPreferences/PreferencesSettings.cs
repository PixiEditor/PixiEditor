using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models.UserPreferences
{
    public static class PreferencesSettings
    {
        public static void UpdatePreference(string name, object value)
        {
            Properties.Settings.Default.Reload();
            if (Properties.Settings.Default.Properties[name] != null)
            {
                Properties.Settings.Default.Properties[name].DefaultValue = value;
            }
            else
            {
                Properties.Settings.Default.Properties.Add(new SettingsProperty(name) { DefaultValue = value });
            }

            Properties.Settings.Default.Save();
        }

#nullable enable

        public static T? GetPreference<T>(string name)
        {
            return GetPreference(name, default(T));
        }

        public static T? GetPreference<T>(string name, T? fallbackValue)
        {
            return Properties.Settings.Default.Properties[name] != null
                ? (T)Convert.ChangeType(Properties.Settings.Default.Properties[name].DefaultValue, typeof(T))
                : fallbackValue;
        }
    }
}