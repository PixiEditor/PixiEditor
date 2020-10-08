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
        ///     Gets setting with given type T in toolbar by name.
        /// </summary>
        /// <param name="name">Setting name, non case sensitive</param>
        /// <returns></returns>
        public Setting<T> GetSetting<T>(string name)
        {
            Setting setting =  Settings.FirstOrDefault(currentSetting => string.Equals(currentSetting.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (setting == null)
                return null;

            if (setting is Setting<T> convertedSetting)
                return convertedSetting;

            throw new Exception("Setting has value with unexpected type");
        }

        /// <summary>
        ///     Saves current toolbar state, so other toolbars with common settings can load them.
        /// </summary>
        public void SaveToolbarSettings()
        {
            SharedSettings.Clear();
            foreach (Setting setting in Settings)
            {
                SharedSettings.Add(setting);
            }
        }

        /// <summary>
        ///     Loads common settings saved from previous tools to current one.
        /// </summary>
        public void LoadSharedSettings()
        {
            Settings.Clear();
            foreach (Setting setting in SharedSettings)
            {
                Settings.Add(setting);
            }
        }
    }
}