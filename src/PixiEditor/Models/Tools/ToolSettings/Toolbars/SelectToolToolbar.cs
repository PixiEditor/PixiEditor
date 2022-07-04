using PixiEditor.Models.Enums;
using PixiEditor.Models.Tools.ToolSettings.Settings;

namespace PixiEditor.Models.Tools.ToolSettings.Toolbars;

public class SelectToolToolbar : Toolbar
{
    public SelectToolToolbar(bool includeSelectionShape = true)
    {
        Settings.Add(new EnumSetting<SelectionType>("SelectMode", "Selection type"));

        if (includeSelectionShape)
        {
            Settings.Add(new EnumSetting<SelectionShape>("SelectShape", "Selection shape"));
        }
    }
}