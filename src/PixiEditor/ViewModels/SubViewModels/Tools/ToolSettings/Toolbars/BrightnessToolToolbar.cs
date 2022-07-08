using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

internal class BrightnessToolToolbar : BasicToolbar
{
    public float CorrectionFactor => GetSetting<FloatSetting>(nameof(CorrectionFactor)).Value;
    public BrightnessMode BrightnessMode => GetSetting<EnumSetting<BrightnessMode>>(nameof(BrightnessMode)).Value;
    public BrightnessToolToolbar(float initialValue)
    {
        Settings.Add(new FloatSetting(nameof(CorrectionFactor), initialValue, "Strength:", 0f, 100f));
        Settings.Add(new EnumSetting<BrightnessMode>(nameof(BrightnessMode), "Mode"));
    }
}
