using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;
using PixiEditor.AvaloniaUI.Models.Handlers.Tools;
using PixiEditor.AvaloniaUI.Views.Overlays.BrushShapeOverlay;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.G)]
internal class FloodFillToolViewModel : ToolViewModel, IFloodFillToolHandler
{
    private readonly string defaultActionDisplay = "FLOOD_FILL_TOOL_ACTION_DISPLAY_DEFAULT";

    public override string ToolNameLocalizationKey => "FLOOD_FILL_TOOL";
    public override BrushShape BrushShape => BrushShape.Pixel;

    public override LocalizedString Tooltip => new("FLOOD_FILL_TOOL_TOOLTIP", Shortcut);

    public override bool UsesColor => true;

    public override bool IsErasable => true;

    public bool ConsiderAllLayers { get; private set; }

    public FloodFillToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
    }

    public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (ctrlIsDown)
        {
            ConsiderAllLayers = true;
            ActionDisplay = "FLOOD_FILL_TOOL_ACTION_DISPLAY_CTRL";
        }
        else
        {
            ConsiderAllLayers = false;
            ActionDisplay = defaultActionDisplay;
        }
    }

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseFloodFillTool();
    }
}
