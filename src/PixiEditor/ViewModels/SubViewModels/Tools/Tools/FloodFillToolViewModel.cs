using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.G)]
internal class FloodFillToolViewModel : ToolViewModel
{
    private readonly string defaultActionDisplay = "Press on an area to fill it. Hold down Ctrl to consider all layers.";

    public override BrushShape BrushShape => BrushShape.Pixel;

    public override LocalizedString Tooltip => new LocalizedString("FLOOD_FILL_TOOL_TOOLTIP", Shortcut);

    public bool ConsiderAllLayers { get; private set; }

    public FloodFillToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
    }

    public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (ctrlIsDown)
        {
            ConsiderAllLayers = true;
            ActionDisplay = "Press on an area to fill it. Release Ctrl to only consider the current layers.";
        }
        else
        {
            ConsiderAllLayers = false;
            ActionDisplay = defaultActionDisplay;
        }
    }

    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseFloodFillTool();
    }
}
