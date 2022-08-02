using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.W)]
internal class MagicWandToolViewModel : ToolViewModel
{
    public override string Tooltip => $"Magic Wand ({Shortcut}). Flood's the selection";

    public override BrushShape BrushShape => BrushShape.Pixel;

    public MagicWandToolViewModel()
    {
        Toolbar = new MagicWandToolbar();
        ActionDisplay = "Click to flood the selection.";
    }
}
