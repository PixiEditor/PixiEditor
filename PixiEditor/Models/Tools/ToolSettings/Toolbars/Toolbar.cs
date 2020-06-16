using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PixiEditor.Models.Tools.ToolSettings
{
    public abstract class Toolbar
    {
        private static readonly List<Setting> _sharedSettings = new List<Setting>();
        public ObservableCollection<Setting> Settings { get; set; } = new ObservableCollection<Setting>();

        public virtual Setting GetSetting(string name)
        {
            return Settings.FirstOrDefault(x => x.Name == name);
        }

        public virtual Setting[] GetSettings(string name)
        {
            return Settings.Where(x => x.Name == name).ToArray();
        }

        /// <summary>
        ///     Saves current toolbar state, so other toolbars with common settings can load them.
        /// </summary>
        public void SaveToolbarSettings()
        {
            for (int i = 0; i < Settings.Count; i++)
                if (_sharedSettings.Any(x => x.Name == Settings[i].Name))
                    _sharedSettings.First(x => x.Name == Settings[i].Name).Value = Settings[i].Value;
                else
                    _sharedSettings.Add(Settings[i]);
        }

        public void LoadSharedSettings()
        {
            for (int i = 0; i < _sharedSettings.Count; i++)
                if (Settings.Any(x => x.Name == _sharedSettings[i].Name))
                    Settings.First(x => x.Name == _sharedSettings[i].Name).Value = _sharedSettings[i].Value;
        }
    }
}