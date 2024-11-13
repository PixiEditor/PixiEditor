using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

internal class PenToolbar : BasicToolbar, IPenToolbar
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

    public PenToolbar()
    {
        AddSetting(new BoolSettingViewModel(nameof(AntiAliasing), "ANTI_ALIASING_SETTING") { IsExposed = false });
        AddSetting(new PercentSettingViewModel(nameof(Hardness), 0.8f, "HARDNESS_SETTING") { IsExposed = false });
        AddSetting(new PercentSettingViewModel(nameof(Spacing), 0.15f, "SPACING_SETTING") { IsExposed = false });
    }
}
