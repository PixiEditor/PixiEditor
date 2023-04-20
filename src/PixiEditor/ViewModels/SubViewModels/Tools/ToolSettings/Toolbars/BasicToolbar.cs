using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

/// <summary>
///     Toolbar with size setting.
/// </summary>
internal class BasicToolbar : Toolbar
{
    public int ToolSize
    {
        get => GetSetting<SizeSetting>(nameof(ToolSize)).Value;
        set => GetSetting<SizeSetting>(nameof(ToolSize)).Value = value;
    }
    public BasicToolbar()
    {
        var setting = new SizeSetting(nameof(ToolSize), "Tool size:");
        setting.ValueChanged += (_, _) => RaisePropertyChanged(nameof(ToolSize));
        Settings.Add(setting);
    }

    public override void OnLoadedSettings()
    {
        RaisePropertyChanged(nameof(ToolSize));
    }
}
