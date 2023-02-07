using System.Windows.Input;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.ToolAttribute(Key = Key.Q)]
internal class LassoToolViewModel : ToolViewModel
{
    private string defaultActionDisplay = "Click and move to select pixels inside of the lasso. Hold Shift to add to existing selection. Hold Ctrl to subtract from it.";

    public LassoToolViewModel()
    {
        Toolbar = ToolbarFactory.Create<LassoToolViewModel>();
        ActionDisplay = defaultActionDisplay;
    }

    private SelectionMode modifierKeySelectionMode = SelectionMode.New;
    public SelectionMode ResultingSelectionMode => modifierKeySelectionMode != SelectionMode.New ? modifierKeySelectionMode : SelectMode;

    public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (shiftIsDown)
        {
            ActionDisplay = "Click and move to add pixels inside of the lasso to the selection.";
            modifierKeySelectionMode = SelectionMode.Add;
        }
        else if (ctrlIsDown)
        {
            ActionDisplay = "Click and move to subtract pixels inside of the lasso from the selection.";
            modifierKeySelectionMode = SelectionMode.Subtract;
        }
        else
        {
            ActionDisplay = defaultActionDisplay;
            modifierKeySelectionMode = SelectionMode.New;
        }
    }

    public override string Tooltip => $"Lasso. ({Shortcut})";
    
    public override BrushShape BrushShape => BrushShape.Pixel;

    [Settings.Enum("Mode")]
    public SelectionMode SelectMode => GetValue<SelectionMode>();
    
    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseLassoTool();
    }
}
