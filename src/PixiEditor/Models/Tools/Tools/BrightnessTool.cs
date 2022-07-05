using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools.Tools;

[Command.Tool(Key = Key.U)]
public class BrightnessTool : BitmapOperationTool
{
    private const float CorrectionFactor = 5f; // Initial correction factor

    private readonly string defaultActionDisplay = "Draw on pixels to make them brighter. Hold Ctrl to darken.";

    public BrightnessTool()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = new BrightnessToolToolbar(CorrectionFactor);
    }

    public override string Tooltip => $"Makes pixels brighter or darker ({Shortcut}). Hold Ctrl to make pixels darker.";

    public BrightnessMode Mode { get; set; } = BrightnessMode.Default;

    public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (!ctrlIsDown)
            ActionDisplay = defaultActionDisplay;
        else
            ActionDisplay = "Draw on pixels to make them darker. Release Ctrl to brighten.";
    }

    public override void Use()
    {

    }
}
