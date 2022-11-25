using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.M)]
internal class SelectToolViewModel : ToolViewModel
{
    public SelectToolViewModel()
    {
        ActionDisplay = "Click and move to select an area.";
        Toolbar = ToolbarFactory.Create<SelectToolViewModel>();
        Cursor = Cursors.Cross;
    }

    [Settings.Enum("Mode")]
    public SelectionMode SelectMode => GetValue<SelectionMode>();

    [Settings.Enum("Shape")]
    public SelectionShape SelectShape => GetValue<SelectionShape>();
    
    public override BrushShape BrushShape => BrushShape.Pixel;

    public override string Tooltip => $"Selects area. ({Shortcut})";

    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseSelectTool();
    }
}
