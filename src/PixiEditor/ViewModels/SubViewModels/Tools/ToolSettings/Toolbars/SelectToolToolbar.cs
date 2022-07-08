using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

internal class SelectToolToolbar : Toolbar
{
    public SelectionMode SelectMode => GetSetting<EnumSetting<SelectionMode>>(nameof(SelectMode)).Value;
    public SelectionShape SelectShape => GetSetting<EnumSetting<SelectionShape>>(nameof(SelectShape)).Value;

    public SelectToolToolbar()
    {
        Settings.Add(new EnumSetting<SelectionMode>(nameof(SelectMode), "Selection type"));
        Settings.Add(new EnumSetting<SelectionShape>(nameof(SelectShape), "Selection shape"));
    }
}
