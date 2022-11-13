using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.O, Transient = Key.LeftAlt)]
internal class ColorPickerToolViewModel : ToolViewModel
{
    private readonly string defaultActionDisplay = "Click to pick colors. Hold Ctrl to hide the canvas. Hold Shift to hide the reference layer";

    public override bool HideHighlight => true;

    public override BrushShape BrushShape => BrushShape.Pixel;

    public override string Tooltip => $"Picks the primary color from the canvas. ({Shortcut})";

    private bool pickFromCanvas = true;
    public bool PickFromCanvas
    {
        get => pickFromCanvas; 
        private set => SetProperty(ref pickFromCanvas, value);
    }
    
    private bool pickFromReferenceLayer = true;
    public bool PickFromReferenceLayer
    {
        get => pickFromReferenceLayer; 
        private set => SetProperty(ref pickFromReferenceLayer, value);
    }

    [Settings.Enum("Scope", DocumentScope.AllLayers)]
    public DocumentScope Mode => GetValue<DocumentScope>();

    public ColorPickerToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = ToolbarFactory.Create<ColorPickerToolViewModel, EmptyToolbar>();
    }

    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseColorPickerTool();
    }

    public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (ctrlIsDown)
        {
            PickFromCanvas = false;
            PickFromReferenceLayer = true;
            ActionDisplay = "Click to pick colors from the reference layer.";
        }
        else if (shiftIsDown)
        {
            PickFromCanvas = true;
            PickFromReferenceLayer = false;
            ActionDisplay = "Click to pick colors from the canvas.";
            return;
        }
        else
        {
            PickFromCanvas = true;
            PickFromReferenceLayer = true;
            ActionDisplay = defaultActionDisplay;
        }
    }
}
