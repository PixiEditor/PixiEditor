using Avalonia.Media;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
#nullable enable
internal class FillableShapeToolbar : ShapeToolbar, IFillableShapeToolbar
{
    public bool Fill
    {
        get
        {
            return GetSetting<BoolSettingViewModel>(nameof(Fill)).Value;
        }
        set
        {
            GetSetting<BoolSettingViewModel>(nameof(Fill)).Value = value;
        }
    }

    public Color FillColor
    {
        get
        {
            return GetSetting<ColorSettingViewModel>(nameof(FillColor)).Value;
        }
        set
        {
            GetSetting<ColorSettingViewModel>(nameof(FillColor)).Value = value;
        }
    }

    public FillableShapeToolbar()
    {
        AddSetting(new BoolSettingViewModel(nameof(Fill), "FILL_SHAPE_LABEL") { Value = true });
        AddSetting(new ColorSettingViewModel(nameof(FillColor), "FILL_COLOR_LABEL"));
    }
}
