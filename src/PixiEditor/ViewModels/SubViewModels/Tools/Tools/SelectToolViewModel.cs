using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.M)]
internal class SelectToolViewModel : ToolViewModel
{
    public SelectToolViewModel()
    {
        ActionDisplay = "Click and move to select an area.";
        Toolbar = new SelectToolToolbar();
        Cursor = Cursors.Cross;
    }

    public SelectionMode SelectionType { get; set; } = SelectionMode.Add;

    public override BrushShape BrushShape => BrushShape.Pixel;

    public override string Tooltip => $"Selects area. ({Shortcut})";

    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseSelectTool();
    }
}
