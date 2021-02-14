using System;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.UserPreferences
{
    public interface IPreferences
    {
        public static IPreferences Current => ViewModelMain.Current.Preferences;

        public void Save();

        public void AddCallback(string setting, Action<object> action);

        public void Init();

        public void Init(string path, string localPath);

        public void UpdatePreference<T>(string name, T value);

        public void UpdateLocalPreference<T>(string name, T value);

#nullable enable

        public T? GetPreference<T>(string name);

        public T? GetPreference<T>(string name, T? fallbackValue);

        public T? GetLocalPreference<T>(string name);

        public T? GetLocalPreference<T>(string name, T? fallbackValue);
    }
}