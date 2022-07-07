using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

internal class BasicShapeToolbar : BasicToolbar
{
    public BasicShapeToolbar()
    {
        Settings.Add(new BoolSetting("Fill", "Fill shape: "));
        Settings.Add(new ColorSetting("FillColor", "Fill color"));
    }
}
