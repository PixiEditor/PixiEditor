using Avalonia.Media;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
#nullable enable
internal class BasicShapeToolbar : BasicToolbar, IBasicShapeToolbar
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

    public bool SyncWithPrimaryColor
    {
        get
        {
            return GetSetting<BoolSettingViewModel>(nameof(SyncWithPrimaryColor)).Value;
        }
        set
        {
            GetSetting<BoolSettingViewModel>(nameof(SyncWithPrimaryColor)).Value = value;
        }
    } 

    public BasicShapeToolbar()
    {
        AddSetting(new ColorSettingViewModel(nameof(StrokeColor), "STROKE_COLOR_LABEL"));
        AddSetting(new BoolSettingViewModel(nameof(Fill), "FILL_SHAPE_LABEL") { Value = true });
        AddSetting(new ColorSettingViewModel(nameof(FillColor), "FILL_COLOR_LABEL"));
        AddSetting(new BoolSettingViewModel(nameof(SyncWithPrimaryColor), "SYNC_WITH_PRIMARY_COLOR_LABEL") { Value = true });
    }
}
