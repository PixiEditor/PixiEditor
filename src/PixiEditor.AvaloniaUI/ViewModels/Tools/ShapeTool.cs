using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Handlers.Tools;
using PixiEditor.AvaloniaUI.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.AvaloniaUI.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.AvaloniaUI.ViewModels.Tools;

internal abstract class ShapeTool : ToolViewModel, IShapeToolHandler
{
    public override BrushShape BrushShape => BrushShape.Hidden;

    public override bool UsesColor => true;

    public override bool IsErasable => true;

    public ShapeTool()
    {
        Cursor = new Cursor(StandardCursorType.Cross);
        Toolbar = new BasicShapeToolbar();
    }
}
