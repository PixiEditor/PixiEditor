using PixiEditor.Models.Tools.ToolSettings.Settings;

namespace PixiEditor.Models.Tools.ToolSettings.Toolbars;

internal class PenToolbar : BasicToolbar
{
    public PenToolbar()
    {
        Settings.Add(new BoolSetting("PixelPerfectEnabled", "Pixel perfect"));
    }
}
