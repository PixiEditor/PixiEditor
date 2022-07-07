using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

internal class MagicWandToolbar : SelectToolToolbar
{
    public MagicWandToolbar()
        : base(false)
    {
        Settings.Add(new EnumSetting<DocumentScope>(nameof(DocumentScope), "Scope"));
    }
}
