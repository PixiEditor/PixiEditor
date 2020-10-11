using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PixiEditor.Models.Tools.ToolSettings.Settings;

namespace PixiEditor.Models.Tools.ToolSettings.Toolbars
{
    public abstract class Toolbar
    {
        private static readonly List<Setting> SharedSettings = new List<Setting>();
        public ObservableCollection<Setting> Settings { get; set; } = new ObservableCollection<Setting>();

        /// <summary>
        ///     Gets setting in toolbar by name.
        /// </summary>
        /// <param name="name">Setting name, non case sensitive</param>
        /// <returns></returns>
        public virtual Setting GetSetting(string name)
        {
            return Settings.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        ///     Gets setting of given type T in toolbar by name.
        /// </summary>
        /// <param name="name">Setting name, non case sensitive</param>
        /// <returns></returns>
        public T GetSetting<T>(string name)
            where T : Setting
        {
            Setting setting =  Settings.FirstOrDefault(currentSetting => string.Equals(currentSetting.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (setting == null || !(setting is T convertedSetting))
                return null;

            return convertedSetting;
        }

        /// <summary>
        ///     Saves current toolbar state, so other toolbars with common settings can load them.
        /// </summary>
        public void SaveToolbarSettings()
        {
            foreach (Setting setting in Settings)
                AddSettingToCollection(SharedSettings, setting);
        }

        /// <summary>
        ///     Loads common settings saved from previous tools to current one.
        /// </summary>
        public void LoadSharedSettings()
        {
            foreach (Setting sharedSetting in SharedSettings)
                AddSettingToCollection(Settings, sharedSetting);
        }

        private static void AddSettingToCollection(ICollection<Setting> collection, Setting setting)
        {
            Setting storedSetting = collection.FirstOrDefault(currentSetting => currentSetting.Name == setting.Name);
            if (storedSetting != null)
                collection.Remove(storedSetting);

            collection.Add(setting);
        }
    }
}