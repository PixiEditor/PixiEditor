using System.Windows.Input;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.ToolAttribute(Key = Key.L)]
internal class LassoToolViewModel : ToolViewModel
{
    public override string Tooltip => $"Lasso. ({Shortcut})";
    
    public override BrushShape BrushShape => BrushShape.Pixel;

    public LassoToolViewModel()
    {
        Toolbar = new LassoToolbar();
        ActionDisplay = "Click and move to select pixels inside of lasso.";
    }

    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseLassoTool();
    }
}
