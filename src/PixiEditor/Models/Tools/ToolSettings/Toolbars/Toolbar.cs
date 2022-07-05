using System.Collections.ObjectModel;
using PixiEditor.Models.Tools.ToolSettings.Settings;

namespace PixiEditor.Models.Tools.ToolSettings.Toolbars;

internal abstract class Toolbar
{
    private static readonly List<Setting> SharedSettings = new List<Setting>();

    public ObservableCollection<Setting> Settings { get; set; } = new ObservableCollection<Setting>();
    public bool SettingsGenerated { get; private set; }

    public void GenerateSettings()
    {
        foreach (var setting in Settings)
        {
            setting.SettingControl = setting.GenerateControl();
        }

        SettingsGenerated = true;
    }

    /// <summary>
    ///     Gets setting in toolbar by name.
    /// </summary>
    /// <param name="name">Setting name, non case sensitive.</param>
    /// <returns>Generic Setting.</returns>
    public virtual Setting GetSetting(string name)
    {
        return Settings.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    ///     Gets setting of given type T in toolbar by name.
    /// </summary>
    /// <param name="name">Setting name, non case sensitive.</param>
    /// <returns>Setting of given type.</returns>
    public T GetSetting<T>(string name)
        where T : Setting
    {
        Setting setting = Settings.FirstOrDefault(currentSetting => string.Equals(currentSetting.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (setting == null || !(setting is T convertedSetting))
        {
            return null;
        }

        return convertedSetting;
    }

    /// <summary>
    ///     Saves current toolbar state, so other toolbars with common settings can load them.
    /// </summary>
    public void SaveToolbarSettings()
    {
        for (int i = 0; i < Settings.Count; i++)
        {
            if (SharedSettings.Any(x => x.Name == Settings[i].Name))
            {
                SharedSettings.First(x => x.Name == Settings[i].Name).Value = Settings[i].Value;
            }
            else
            {
                SharedSettings.Add(Settings[i]);
            }
        }
    }

    /// <summary>
    ///     Loads common settings saved from previous tools to current one.
    /// </summary>
    public void LoadSharedSettings()
    {
        for (int i = 0; i < SharedSettings.Count; i++)
        {
            if (Settings.Any(x => x.Name == SharedSettings[i].Name))
            {
                Settings.First(x => x.Name == SharedSettings[i].Name).Value = SharedSettings[i].Value;
            }
        }
    }
}
