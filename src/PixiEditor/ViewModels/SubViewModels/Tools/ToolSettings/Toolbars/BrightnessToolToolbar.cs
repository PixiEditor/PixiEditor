using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

internal class BrightnessToolToolbar : BasicToolbar
{
    public BrightnessToolToolbar(float initialValue)
    {
        Settings.Add(new FloatSetting("CorrectionFactor", initialValue, "Strength:", 0f, 100f));
        Settings.Add(new EnumSetting<BrightnessMode>("BrightnessMode", "Mode"));
    }
}
