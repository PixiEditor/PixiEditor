using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

internal class SelectToolToolbar : Toolbar
{
    public SelectToolToolbar(bool includeSelectionShape = true)
    {
        Settings.Add(new EnumSetting<SelectionMode>("SelectMode", "Selection type"));

        if (includeSelectionShape)
        {
            Settings.Add(new EnumSetting<SelectionShape>("SelectShape", "Selection shape"));
        }
    }
}
