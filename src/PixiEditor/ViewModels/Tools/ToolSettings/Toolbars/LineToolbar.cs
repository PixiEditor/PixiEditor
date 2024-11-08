using Avalonia.Media;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

internal class LineToolbar : BasicToolbar, ILineToolbar
{
    
    public Color StrokeColor
    {
        get
        {
            return GetSetting<ColorSettingViewModel>(nameof(StrokeColor)).Value;
        }
        set
        {
            GetSetting<ColorSettingViewModel>(nameof(StrokeColor)).Value = value;
        }
    }

    public bool AntiAliasing
    {
        get
        {
            return GetSetting<BoolSettingViewModel>(nameof(AntiAliasing)).Value;
        }
        set
        {
            GetSetting<BoolSettingViewModel>(nameof(AntiAliasing)).Value = value;
        }
    }

    public LineToolbar()
    {
        AddSetting(new ColorSettingViewModel(nameof(StrokeColor), "STROKE_COLOR_LABEL"));
        AddSetting(new BoolSettingViewModel(nameof(AntiAliasing), "ANTI_ALIASING_LABEL") { IsExposed = false, Value = false });
    }
}
