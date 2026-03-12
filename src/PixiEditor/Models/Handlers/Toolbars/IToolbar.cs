using System.Collections.ObjectModel;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.Models.Handlers.Toolbars;

public delegate void SettingChange(string name, object value);
internal interface IToolbar : IHandler
{
    public void AddSetting(Setting setting);
    public Setting GetSetting(string name);
    public T GetSetting<T>(string name) where T : Setting;
    public IReadOnlyList<Setting> Settings { get; }
    public void SaveToolbarSettings();
    public void LoadSharedSettings();
    public event SettingChange SettingChanged;
    public void RemoveSetting(Setting setting);
}
