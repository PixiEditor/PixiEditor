using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

internal class PenToolbar : Toolbar, IPenToolbar
{
    public bool AntiAliasing
    {
        get => GetSetting<BoolSettingViewModel>(nameof(AntiAliasing)).Value;
        set => GetSetting<BoolSettingViewModel>(nameof(AntiAliasing)).Value = value;
    }

    public float Hardness
    {
        get => GetSetting<PercentSettingViewModel>(nameof(Hardness)).Value;
        set => GetSetting<PercentSettingViewModel>(nameof(Hardness)).Value = value;
    }

    public float Spacing
    {
        get => GetSetting<PercentSettingViewModel>(nameof(Spacing)).Value;
        set => GetSetting<PercentSettingViewModel>(nameof(Spacing)).Value = value;
    }

    public double ToolSize
    {
        get => GetSetting<SizeSettingViewModel>(nameof(ToolSize)).Value;
        set => GetSetting<SizeSettingViewModel>(nameof(ToolSize)).Value = value;
    }

    public PenBrushShape PenShape
    {
        get => GetSetting<EnumSettingViewModel<PenBrushShape>>(nameof(PenShape)).Value;
        set => GetSetting<EnumSettingViewModel<PenBrushShape>>(nameof(PenShape)).Value = value;
    }

    public override void OnLoadedSettings()
    {
        OnPropertyChanged(nameof(ToolSize));
    }

    public PenToolbar()
    {
        AddSetting(new BoolSettingViewModel(nameof(AntiAliasing), "ANTI_ALIASING_SETTING") { IsExposed = false });
        AddSetting(new PercentSettingViewModel(nameof(Hardness), 0.8f, "HARDNESS_SETTING") { IsExposed = false });
        AddSetting(new PercentSettingViewModel(nameof(Spacing), 0.15f, "SPACING_SETTING") { IsExposed = false });
        var setting = new SizeSettingViewModel(nameof(ToolSize), "TOOL_SIZE_LABEL");
        setting.ValueChanged += (_, _) => OnPropertyChanged(nameof(ToolSize));
        AddSetting(setting);
        AddSetting(new EnumSettingViewModel<PenBrushShape>(nameof(PenShape), "PEN_SHAPE_SETTING") { IsExposed = false });
    }
}
