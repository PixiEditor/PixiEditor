using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

internal abstract class Toolbar : ObservableObject, IToolbar
{
    private static readonly List<Setting> SharedSettings = new List<Setting>();

    private ObservableCollection<Setting> settings = new();
    public IReadOnlyList<Setting> Settings => settings; 

    public void AddSetting(Setting setting)
    {
        setting.ValueChanged += (sender, args) =>
        {
            if (args.OldValue != args.NewValue)
            {
                SettingChanged?.Invoke(setting.Name, setting.Value);
            }
        };
        
        settings.Add(setting);
    }

    /// <summary>
    ///     Gets setting in toolbar by name.
    /// </summary>
    /// <param name="name">Setting name, non case sensitive.</param>
    /// <returns>Generic Setting.</returns>
    public virtual Setting? GetSetting(string name)
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

        if (setting is not T convertedSetting)
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
                SharedSettings.First(x => x.Name == Settings[i].Name).UserValue = Settings[i].UserValue;
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
                Settings.First(x => x.Name == SharedSettings[i].Name).UserValue = SharedSettings[i].UserValue;
            }
        }

        OnLoadedSettings();
    }

    public event SettingChange? SettingChanged;
    public void RemoveSetting(Setting setting)
    {
        settings.Remove(setting);
    }

    public virtual void OnLoadedSettings()
    {
    }
}
