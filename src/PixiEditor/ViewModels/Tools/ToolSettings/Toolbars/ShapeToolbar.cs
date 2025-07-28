using Avalonia.Media;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

internal class ShapeToolbar : Toolbar, IShapeToolbar
{
    public double ToolSize
    {
        get
        {
            return GetSetting<SizeSettingViewModel>(nameof(ToolSize)).Value;
        }
        set
        {
            GetSetting<SizeSettingViewModel>(nameof(ToolSize)).Value = value;
        }
    }

    public IBrush StrokeBrush
    {
        get
        {
            return GetSetting<ColorSettingViewModel>(nameof(StrokeBrush)).Value;
        }
        set
        {
            GetSetting<ColorSettingViewModel>(nameof(StrokeBrush)).Value = value;
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

    public bool SyncWithPrimaryColor
    {
        get => GetSetting<BoolSettingViewModel>(nameof(SyncWithPrimaryColor)).Value;
        set
        {
            GetSetting<BoolSettingViewModel>(nameof(SyncWithPrimaryColor)).Value = value;
        }
    }

    public ShapeToolbar()
    {
        AddSetting(new SizeSettingViewModel(nameof(ToolSize), "STROKE_WIDTH", 0, min: 0, decimalPlaces: 2));
        AddSetting(new BoolSettingViewModel(nameof(AntiAliasing), "ANTI_ALIASING_LABEL")
        {
            IsExposed = false, Value = false
        });
        AddSetting(
            new BoolSettingViewModel(nameof(SyncWithPrimaryColor))
                { Value = true, Icon = PixiPerfectIcons.LinkedPipette,
                    Tooltip = "SYNC_WITH_PRIMARY_COLOR_LABEL"});
        AddSetting(new ColorSettingViewModel(nameof(StrokeBrush), "STROKE_COLOR_LABEL"));
    }
}
