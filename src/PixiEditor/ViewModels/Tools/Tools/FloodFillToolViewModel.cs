using Avalonia.Input;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using Drawie.Numerics;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.G)]
internal class FloodFillToolViewModel : ToolViewModel, IFloodFillToolHandler
{
    private readonly string defaultActionDisplay = "FLOOD_FILL_TOOL_ACTION_DISPLAY_DEFAULT";

    public override string ToolNameLocalizationKey => "FLOOD_FILL_TOOL";

    //TODO: Brush Shape was Pixel
    public override Type[]? SupportedLayerTypes { get; } = { typeof(IRasterLayerHandler) };

    public override LocalizedString Tooltip => new("FLOOD_FILL_TOOL_TOOLTIP", Shortcut);

    public override bool UsesColor => true;

    public override bool IsErasable => true;

    public bool ConsiderAllLayers { get; private set; }

    [Settings.Percent("TOLERANCE_LABEL", ExposedByDefault = false)]
    public float Tolerance => GetValue<float>();

    public override string DefaultIcon => PixiPerfectIcons.Bucket;

    public FloodFillToolViewModel()
    {
        Toolbar = ToolbarFactory.Create(this);
        ActionDisplay = defaultActionDisplay;
    }

    public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(ImageLayerNode);

    public override void KeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown, Key argsKey)
    {
        if (ctrlIsDown)
        {
            ConsiderAllLayers = true;
            ActionDisplay = "FLOOD_FILL_TOOL_ACTION_DISPLAY_CTRL";
        }
        else
        {
            ConsiderAllLayers = false;
            ActionDisplay = defaultActionDisplay;
        }
    }

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseFloodFillTool();
    }
}
