using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.SDK
{
    public class Preferences
    {
        private static Action<Extension, string, object, PreferenceStorageLocation> SavePreferenceCallback { get; set; }

        private static Func<Extension, string, PreferenceStorageLocation, object> LoadPreferenceCallback { get; set; }

        private Extension Extension { get; set; }

        internal static void Init(Action<Extension, string, object, PreferenceStorageLocation> savePreference, Func<Extension, string, PreferenceStorageLocation, object> loadPreference)
        {
            SavePreferenceCallback = savePreference;
        }

        internal Preferences(Extension extension)
        {
            Extension = extension;
        }

        public void UpdatePreference<T>(string name, T value)
        {
            SavePreferenceCallback(Extension, name, value, PreferenceStorageLocation.Local);

            if (Callbacks.ContainsKey((name, PreferenceStorageLocation.Roaming)))
            {
                foreach (var action in Callbacks[(name, PreferenceStorageLocation.Roaming)])
                {
                    action.Invoke(value);
                }
            }
        }

        public void UpdateLocalPreference<T>(string name, T value)
        {
            SavePreferenceCallback(Extension, name, value, PreferenceStorageLocation.Local);

            if (Callbacks.ContainsKey((name, PreferenceStorageLocation.Local)))
            {
                foreach (var action in Callbacks[(name, PreferenceStorageLocation.Local)])
                {
                    action.Invoke(value);
                }
            }
        }

        private Dictionary<(string, PreferenceStorageLocation), List<Action<object>>> Callbacks { get; set; } = new Dictionary<(string, PreferenceStorageLocation), List<Action<object>>>();

        public void AddCallback(string name, Action<object> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (Callbacks.ContainsKey((name, PreferenceStorageLocation.Local)))
            {
                Callbacks[(name, PreferenceStorageLocation.Roaming)].Add(action);
                return;
            }

            Callbacks.Add((name, PreferenceStorageLocation.Roaming), new List<Action<object>>() { action });
        }

        public void AddCallback<T>(string name, Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            AddCallback(name, new Action<object>(o => action((T)o)));
        }

        public void AddLocalCallback(string name, Action<object> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (Callbacks.ContainsKey((name, PreferenceStorageLocation.Local)))
            {
                Callbacks[(name, PreferenceStorageLocation.Local)].Add(action);
                return;
            }

            Callbacks.Add((name, PreferenceStorageLocation.Local), new List<Action<object>>() { action });
        }

        public void AddLocalCallback<T>(string name, Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            AddLocalCallback(name, new Action<object>(o => action((T)o)));
        }

#nullable enable

        public T? GetPreference<T>(string name)
        {
            return GetLocalPreference(name, default(T));
        }

        public T? GetPreference<T>(string name, T? fallbackValue)
        {
            T? obj = (T?)Convert.ChangeType(LoadPreferenceCallback(Extension, name, PreferenceStorageLocation.Roaming), typeof(T));

            if (obj == null)
            {
                return fallbackValue;
            }

            return obj;
        }

        public T? GetLocalPreference<T>(string name)
        {
            return GetLocalPreference(name, default(T));
        }

        public T? GetLocalPreference<T>(string name, T? fallbackValue)
        {
            T? obj = (T?)Convert.ChangeType(LoadPreferenceCallback(Extension, name, PreferenceStorageLocation.Local), typeof(T));

            if (obj == null)
            {
                return fallbackValue;
            }

            return obj;
        }

    }
}
