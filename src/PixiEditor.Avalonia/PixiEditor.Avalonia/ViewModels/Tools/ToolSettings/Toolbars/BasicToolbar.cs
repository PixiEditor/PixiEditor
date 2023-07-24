using PixiEditor.Models.Containers.Toolbars;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

/// <summary>
///     Toolbar with size setting.
/// </summary>
internal class BasicToolbar : Toolbar, IBasicToolbar
{
    public int ToolSize
    {
        get => GetSetting<SizeSetting>(nameof(ToolSize)).Value;
        set => GetSetting<SizeSetting>(nameof(ToolSize)).Value = value;
    }
    public BasicToolbar()
    {
        var setting = new SizeSetting(nameof(ToolSize), "TOOL_SIZE_LABEL");
        setting.ValueChanged += (_, _) => OnPropertyChanged(nameof(ToolSize));
        Settings.Add(setting);
    }

    public override void OnLoadedSettings()
    {
        OnPropertyChanged(nameof(ToolSize));
    }
}
