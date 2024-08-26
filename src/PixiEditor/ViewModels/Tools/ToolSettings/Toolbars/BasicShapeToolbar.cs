using Avalonia.Media;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
#nullable enable
internal class BasicShapeToolbar : BasicToolbar, IBasicShapeToolbar
{
    public bool Fill => GetSetting<BoolSettingViewModel>(nameof(Fill)).Value;
    public Color FillColor => GetSetting<ColorSettingViewModel>(nameof(FillColor)).Value;

    public BasicShapeToolbar()
    {
        Settings.Add(new BoolSettingViewModel(nameof(Fill), "FILL_SHAPE_LABEL"));
        Settings.Add(new ColorSettingViewModel(nameof(FillColor), "FILL_COLOR_LABEL"));
    }
}
