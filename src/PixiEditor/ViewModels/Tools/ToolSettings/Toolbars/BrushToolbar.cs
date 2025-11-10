using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

internal class BrushToolbar : Toolbar, IBrushToolbar
{
    public bool AntiAliasing
    {
        get => GetSetting<BoolSettingViewModel>(nameof(AntiAliasing)).Value;
        set => GetSetting<BoolSettingViewModel>(nameof(AntiAliasing)).Value = value;
    }

    public double ToolSize
    {
        get => GetSetting<SizeSettingViewModel>(nameof(ToolSize)).Value;
        set => GetSetting<SizeSettingViewModel>(nameof(ToolSize)).Value = value;
    }

    public Brush Brush
    {
        get => GetSetting<BrushSettingViewModel>(nameof(Brush)).Value;
        set => GetSetting<BrushSettingViewModel>(nameof(Brush)).Value = value;
    }

    public override void OnLoadedSettings()
    {
        OnPropertyChanged(nameof(ToolSize));
    }

    public BrushToolbar()
    {
        AddSetting(new BoolSettingViewModel(nameof(AntiAliasing), "ANTI_ALIASING_SETTING") { IsExposed = false });
        var setting = new SizeSettingViewModel(nameof(ToolSize), "TOOL_SIZE_LABEL", decimalPlaces: 1, min: 0.1);
        setting.ValueChanged += (_, _) => OnPropertyChanged(nameof(ToolSize));
        AddSetting(setting);
        AddSetting(new BrushSettingViewModel(nameof(Brush), "BRUSH_SETTING") { IsExposed = true });
    }
}
