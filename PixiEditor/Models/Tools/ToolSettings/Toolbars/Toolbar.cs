using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PixiEditor.Models.Tools.ToolSettings
{
    public abstract class Toolbar
    {
        public ObservableCollection<Setting> Settings { get; set; } = new ObservableCollection<Setting>();
        private static List<Setting> _sharedSettings = new List<Setting>();

        public virtual Setting GetSetting(string name)
        {
            return Settings.FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// Saves current toolbar state, so other toolbars with common settings can load them.
        /// </summary>
        public void SaveToolbarSettings()
        {
            for (int i = 0; i < Settings.Count; i++)
            {
                if (_sharedSettings.Any(x => x.Name == Settings[i].Name))
                {
                    _sharedSettings.First(x => x.Name == Settings[i].Name).Value = Settings[i].Value;
                }
                else
                {
                    _sharedSettings.Add(Settings[i]);
                }


            }
        }

        public void LoadSharedSettings()
        {
            for (int i = 0; i < _sharedSettings.Count; i++)
            {
                if (Settings.Any(x => x.Name == _sharedSettings[i].Name))
                {
                    Settings.First(x => x.Name == _sharedSettings[i].Name).Value = _sharedSettings[i].Value;
                }
            }
        }
    }
}
