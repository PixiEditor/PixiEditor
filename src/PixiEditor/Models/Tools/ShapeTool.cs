using System.Windows.Input;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools;

internal abstract class ShapeTool : BitmapOperationTool
{
    public ShapeTool()
    {
        Cursor = Cursors.Cross;
        Toolbar = new BasicShapeToolbar();
    }
}
