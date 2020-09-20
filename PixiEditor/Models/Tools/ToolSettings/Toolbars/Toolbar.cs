using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
        public virtual Setting<T> GetSetting<T>(string name)
        {
            var setting = Settings.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            return setting==null?null:new Setting<T>((T)setting.Value, setting.Name);
        }

        /// <summary>
        ///     Saves current toolbar state, so other toolbars with common settings can load them.
        /// </summary>
        public void SaveToolbarSettings()
        {
            for (int i = 0; i < Settings.Count; i++)
                if (SharedSettings.Any(x => x.Name == Settings[i].Name))
                    SharedSettings.First(x => x.Name == Settings[i].Name).Value = Settings[i].Value;
                else
                    SharedSettings.Add(Settings[i]);
        }

        /// <summary>
        ///     Loads common settings saved from previous tools to current one.
        /// </summary>
        public void LoadSharedSettings()
        {
            for (int i = 0; i < SharedSettings.Count; i++)
                if (Settings.Any(x => x.Name == SharedSettings[i].Name))
                    Settings.First(x => x.Name == SharedSettings[i].Name).Value = SharedSettings[i].Value;
        }
    }
}