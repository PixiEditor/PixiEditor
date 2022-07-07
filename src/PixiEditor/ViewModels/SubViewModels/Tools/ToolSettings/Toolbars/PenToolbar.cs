using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

internal class PenToolbar : BasicToolbar
{
    public PenToolbar()
    {
        Settings.Add(new BoolSetting("PixelPerfectEnabled", "Pixel perfect"));
    }
}
