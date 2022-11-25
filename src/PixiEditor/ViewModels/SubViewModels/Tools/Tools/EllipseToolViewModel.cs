using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.C)]
internal class EllipseToolViewModel : ShapeTool
{
    private string defaultActionDisplay = "Click and move mouse to draw an ellipse. Hold Shift to draw a circle.";

    public EllipseToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
    }

    public override string Tooltip => $"Draws an ellipse on canvas ({Shortcut}). Hold Shift to draw a circle.";
    public bool DrawCircle { get; private set; }

    public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (shiftIsDown)
        {
            ActionDisplay = "Click and move mouse to draw a circle.";
            DrawCircle = true;
        }
        else
        {
            ActionDisplay = defaultActionDisplay;
            DrawCircle = false;
        }
    }

    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseEllipseTool();
    }
}
