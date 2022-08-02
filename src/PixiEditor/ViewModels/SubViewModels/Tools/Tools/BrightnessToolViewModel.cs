using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.U)]
internal class BrightnessToolViewModel : ToolViewModel
{
    private const float CorrectionFactor = 5f;

    private readonly string defaultActionDisplay = "Draw on pixels to make them brighter. Hold Ctrl to darken.";

    public BrightnessToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = new BrightnessToolToolbar(CorrectionFactor);
    }

    public override string Tooltip => $"Makes pixels brighter or darker ({Shortcut}). Hold Ctrl to make pixels darker.";

    public override BrushShape BrushShape => BrushShape.Circle;

    public BrightnessMode Mode { get; set; } = BrightnessMode.Default;

    public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (!ctrlIsDown)
            ActionDisplay = defaultActionDisplay;
        else
            ActionDisplay = "Draw on pixels to make them darker. Release Ctrl to brighten.";
    }
}
