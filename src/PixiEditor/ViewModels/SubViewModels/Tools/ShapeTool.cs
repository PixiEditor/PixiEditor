using System.Windows.Input;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools;

internal abstract class ShapeTool : ToolViewModel
{
    public override BrushShape BrushShape => BrushShape.Pixel;
    public ShapeTool()
    {
        Cursor = Cursors.Cross;
        Toolbar = new BasicShapeToolbar();
    }

    public override void OnDeselected()
    {
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Operations.TryStopToolLinkedExecutor();
    }
}
