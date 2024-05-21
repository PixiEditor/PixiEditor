using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;
using PixiEditor.AvaloniaUI.Models.Handlers.Tools;
using PixiEditor.AvaloniaUI.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.AvaloniaUI.Views.Overlays.BrushShapeOverlay;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.E)]
internal class EraserToolViewModel : ToolViewModel, IEraserToolHandler
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
