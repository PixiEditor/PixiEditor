using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Localization;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.E)]
internal class EraserToolViewModel : ToolViewModel
{
    public EraserToolViewModel()
    {
        ActionDisplay = "ERASER_TOOL_ACTION_DISPLAY";
        Toolbar = ToolbarFactory.Create<EraserToolViewModel, BasicToolbar>(this);
    }

    [Settings.Inherited]
    public int ToolSize => GetValue<int>();

    public override bool IsErasable => true;

    public override string ToolNameLocalizationKey => "ERASER_TOOL";
    public override BrushShape BrushShape => BrushShape.Circle;

    public override LocalizedString Tooltip => new LocalizedString("ERASER_TOOL_TOOLTIP", Shortcut);

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseEraserTool();
    }
}
