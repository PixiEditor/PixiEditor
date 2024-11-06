using Avalonia.Input;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using Drawie.Numerics;
using PixiEditor.UI.Common.Fonts;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.R)]
internal class RasterRectangleToolViewModel : ShapeTool, IRasterRectangleToolHandler
{
    private string defaultActionDisplay = "RECTANGLE_TOOL_ACTION_DISPLAY_DEFAULT";
    public RasterRectangleToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
    }

    public override string ToolNameLocalizationKey => "RECTANGLE_TOOL";
    public override Type[]? SupportedLayerTypes { get; } = { typeof(IRasterLayerHandler) };
    public override LocalizedString Tooltip => new LocalizedString("RECTANGLE_TOOL_TOOLTIP", Shortcut);

    public bool Filled { get; set; } = false;
    public bool DrawSquare { get; private set; } = false;

    public override string Icon => PixiPerfectIcons.Square;

    public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(ImageLayerNode);

    public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (shiftIsDown)
        {
            DrawSquare = true;
            ActionDisplay = "RECTANGLE_TOOL_ACTION_DISPLAY_SHIFT";
        }
        else
        {
            DrawSquare = false;
            ActionDisplay = defaultActionDisplay;
        }
    }

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseRasterRectangleTool();
    }

    public override void OnSelected(bool restoring)
    {
        if(restoring) return;
        
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseRasterRectangleTool();
    }
}
