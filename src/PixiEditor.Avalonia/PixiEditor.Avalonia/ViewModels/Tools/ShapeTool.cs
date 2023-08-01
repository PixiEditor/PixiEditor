using System.Windows.Input;
using Avalonia.Input;
using PixiEditor.Models.Containers.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools;

internal abstract class ShapeTool : ToolViewModel, IShapeToolHandler
{
    public override BrushShape BrushShape => BrushShape.Pixel;

    public override bool UsesColor => true;

    public override bool IsErasable => true;

    public ShapeTool()
    {
        Cursor = new Cursor(StandardCursorType.Cross);
        Toolbar = new BasicShapeToolbar();
    }
}
