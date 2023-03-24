using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.V)]
internal class MoveToolViewModel : ToolViewModel
{
    private string defaultActionDisplay = new LocalizedString("MOVE_TOOL_ACTION_DISPLAY");

    public MoveToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = ToolbarFactory.Create<MoveToolViewModel>();
        Cursor = Cursors.Arrow;
    }

    public override LocalizedString Tooltip => new LocalizedString("MOVE_TOOL_TOOLTIP", Shortcut);

    [Settings.Bool("KEEP_ORIGINAL_IMAGE_SETTING")]
    public bool KeepOriginalImage => GetValue<bool>();
    
    public override BrushShape BrushShape => BrushShape.Hidden;
    public override bool HideHighlight => true;

    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseShiftLayerTool();
    }

    public override void OnSelected()
    {
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Operations.TransformSelectedArea(true);
    }
}
