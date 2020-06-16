using PixiEditor.Models.Tools.ToolSettings.Settings;

namespace PixiEditor.Models.Tools.ToolSettings
{
    public class BasicToolbar : Toolbar
    {
        public BasicToolbar()
        {
            Settings.Add(new SizeSetting("ToolSize", "Tool size:"));
        }
    }
}