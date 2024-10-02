using Avalonia.Input;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Numerics;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.Tools;

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
    public override Type[]? SupportedLayerTypes { get; } =
    {
        typeof(IRasterLayerHandler)
    };

    public override string Icon => PixiPerfectIcons.Eraser;

    public override LocalizedString Tooltip => new LocalizedString("ERASER_TOOL_TOOLTIP", Shortcut);

    public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(ImageLayerNode);

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseEraserTool();
    }
}
