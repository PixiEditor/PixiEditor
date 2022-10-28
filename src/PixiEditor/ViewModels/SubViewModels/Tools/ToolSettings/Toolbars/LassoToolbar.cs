using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

internal class LassoToolbar : Toolbar
{
    public SelectionMode SelectMode => GetSetting<EnumSetting<SelectionMode>>(nameof(SelectMode)).Value;
    
    public LassoToolbar()
    {
        Settings.Add(new EnumSetting<SelectionMode>(nameof(SelectMode), "Selection type"));
    }
}
