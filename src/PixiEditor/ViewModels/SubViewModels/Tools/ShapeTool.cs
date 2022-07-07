using System.Windows.Input;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.SubViewModels.Tools;

internal abstract class ShapeTool : ToolViewModel
{
    public ShapeTool()
    {
        Cursor = Cursors.Cross;
        Toolbar = new BasicShapeToolbar();
    }
}
