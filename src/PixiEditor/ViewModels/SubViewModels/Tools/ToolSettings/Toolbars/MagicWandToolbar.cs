using System.Windows.Controls;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

internal class MagicWandToolbar : BasicToolbar
{
    public SelectionMode SelectMode => GetSetting<EnumSetting<SelectionMode>>(nameof(SelectMode)).Value;
    public DocumentScope DocumentScope => GetSetting<EnumSetting<DocumentScope>>(nameof(DocumentScope)).Value;
    public MagicWandToolbar()
    {
        Settings.Add(new EnumSetting<SelectionMode>(nameof(SelectMode), "Selection type"));
        Settings.Add(new EnumSetting<DocumentScope>(nameof(DocumentScope), "Scope"));
    }
}
