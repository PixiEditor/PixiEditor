using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.V)]
internal class MoveToolViewModel : ToolViewModel
{
    private string defaultActionDisplay = "Hold mouse to move selected pixels. Hold Ctrl to move all layers.";

    public MoveToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = ToolbarFactory.Create<MoveToolViewModel>();
        Cursor = Cursors.Arrow;
    }

    public override string Tooltip => $"Moves selected pixels ({Shortcut}). Hold Ctrl to move all layers.";

    [Settings.Bool("Keep original image")]
    public bool KeepOriginalImage => GetValue<bool>();
    
    public override BrushShape BrushShape => BrushShape.Hidden;
    public override bool HideHighlight => true;

    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseShiftLayerTool();
    }

    public override void OnSelected()
    {
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Operations.TransformSelectedArea(true);
    }
}
