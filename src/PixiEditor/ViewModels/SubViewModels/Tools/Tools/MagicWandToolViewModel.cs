using System.Windows.Input;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.W)]
internal class MagicWandToolViewModel : ToolViewModel
{
    public override string Tooltip => $"Magic Wand ({Shortcut}). Flood's the selection";

    public override BrushShape BrushShape => BrushShape.Pixel;

    [Settings.Enum("Mode")]
    public SelectionMode SelectMode => GetValue<SelectionMode>();

    [Settings.Enum("Scope")]
    public DocumentScope DocumentScope => GetValue<DocumentScope>();
    
    public MagicWandToolViewModel()
    {
        Toolbar = ToolbarFactory.Create(this);
        ActionDisplay = "Click to flood the selection.";
    }
    
    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseMagicWandTool();
    }
}
