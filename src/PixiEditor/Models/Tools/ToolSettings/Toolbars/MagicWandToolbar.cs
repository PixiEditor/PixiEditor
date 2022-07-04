using PixiEditor.Models.Enums;
using PixiEditor.Models.Tools.ToolSettings.Settings;

namespace PixiEditor.Models.Tools.ToolSettings.Toolbars;

public class MagicWandToolbar : SelectToolToolbar
{
    public MagicWandToolbar()
        : base(false)
    {
        Settings.Add(new EnumSetting<DocumentScope>(nameof(DocumentScope), "Scope"));
    }
}