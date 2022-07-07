using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

/// <summary>
///     Toolbar with size setting.
/// </summary>
internal class BasicToolbar : Toolbar
{
    public BasicToolbar()
    {
        Settings.Add(new SizeSetting("ToolSize", "Tool size:"));
    }
}
