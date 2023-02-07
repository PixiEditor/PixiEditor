using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.M)]
internal class SelectToolViewModel : ToolViewModel
{
    private string defaultActionDisplay = "Click and move to select an area. Hold Shift to add to existing selection. Hold Ctrl to subtract from it.";

    public SelectToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = ToolbarFactory.Create<SelectToolViewModel>();
        Cursor = Cursors.Cross;
    }

    private SelectionMode modifierKeySelectionMode = SelectionMode.New;
    public SelectionMode ResultingSelectionMode => modifierKeySelectionMode != SelectionMode.New ? modifierKeySelectionMode : SelectMode;

    public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (shiftIsDown)
        {
            ActionDisplay = "Click and move to add to the current selection.";
            modifierKeySelectionMode = SelectionMode.Add;
        }
        else if (ctrlIsDown)
        {
            ActionDisplay = "Click and move to subtract from the current selection.";
            modifierKeySelectionMode = SelectionMode.Subtract;
        }
        else
        {
            ActionDisplay = defaultActionDisplay;
            modifierKeySelectionMode = SelectionMode.New;
        }
    }

    [Settings.Enum("Mode")]
    public SelectionMode SelectMode => GetValue<SelectionMode>();

    [Settings.Enum("Shape")]
    public SelectionShape SelectShape => GetValue<SelectionShape>();
    
    public override BrushShape BrushShape => BrushShape.Pixel;

    public override string Tooltip => $"Selects area. ({Shortcut})";

    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseSelectTool();
    }
}
