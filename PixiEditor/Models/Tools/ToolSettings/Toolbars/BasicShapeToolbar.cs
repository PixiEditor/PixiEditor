using PixiEditor.Models.Tools.ToolSettings.Settings;

namespace PixiEditor.Models.Tools.ToolSettings
{
    public class BasicShapeToolbar : BasicToolbar
    {
        public BasicShapeToolbar()
        {
            Settings.Add(new BoolSetting("Fill", "Fill shape: "));
            Settings.Add(new ColorSetting("FillColor", "Fill color"));
        }
    }
}
