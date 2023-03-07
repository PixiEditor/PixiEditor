using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.L)]
internal class LineToolViewModel : ShapeTool
{
    private string defaultActionDisplay = "Click and move to draw a line. Hold Shift to enable snapping.";

    public LineToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = ToolbarFactory.Create<LineToolViewModel, BasicToolbar>();
    }

    public override string Tooltip => $"Draws line on canvas ({Shortcut}). Hold Shift to enable snapping.";

    [Settings.Inherited]
    public int ToolSize => GetValue<int>();

    public bool Snap { get; private set; }

    public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (shiftIsDown)
        {
            ActionDisplay = "Click and move mouse to draw a line with snapping enabled.";
            Snap = true;
        }
        else
        {
            ActionDisplay = defaultActionDisplay;
            Snap = false;
        }
    }

    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseLineTool();
    }
}
