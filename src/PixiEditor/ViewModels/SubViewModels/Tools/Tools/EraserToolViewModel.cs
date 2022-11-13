using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.E)]
internal class EraserToolViewModel : ToolViewModel
{
    public EraserToolViewModel()
    {
        ActionDisplay = "Draw to remove color from a pixel.";
        Toolbar = ToolbarFactory.Create<EraserToolViewModel, BasicToolbar>();
    }

    [Settings.Inherited]
    public int ToolSize => GetValue<int>();

    public override BrushShape BrushShape => BrushShape.Circle;

    public override string Tooltip => $"Erasers color from pixel. ({Shortcut})";

    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseEraserTool();
    }
}
