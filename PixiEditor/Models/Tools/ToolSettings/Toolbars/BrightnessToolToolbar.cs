using PixiEditor.Models.Tools.ToolSettings.Settings;

namespace PixiEditor.Models.Tools.ToolSettings.Toolbars
{
    public class BrightnessToolToolbar : BasicToolbar
    {
        public BrightnessToolToolbar(float initialValue)
        {
            Settings.Add(new FloatSetting("CorrectionFactor", initialValue, "Strength:", 0f, 100f));
        }
    }
}
