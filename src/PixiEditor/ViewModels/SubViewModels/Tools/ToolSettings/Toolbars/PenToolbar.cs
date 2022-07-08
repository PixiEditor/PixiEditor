using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

internal class PenToolbar : BasicToolbar
{
    public bool PixelPerfectEnabled => GetSetting<BoolSetting>(nameof(PixelPerfectEnabled)).Value;
    public PenToolbar()
    {
        Settings.Add(new BoolSetting(nameof(PixelPerfectEnabled), "Pixel perfect"));
    }
}
