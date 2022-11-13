using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.H, Transient = Key.Space)]
internal class MoveViewportToolViewModel : ToolViewModel
{
    public MoveViewportToolViewModel()
    {
        Cursor = Cursors.SizeAll;
        ActionDisplay = "Click and move to pan viewport.";
    }

    public override BrushShape BrushShape => BrushShape.Hidden;
    public override bool HideHighlight => true;
    public override string Tooltip => $"Moves viewport. ({Shortcut})";
}
