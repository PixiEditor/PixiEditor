using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;
using PixiEditor.AvaloniaUI.Models.Handlers.Tools;
using PixiEditor.AvaloniaUI.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.AvaloniaUI.Views.Overlays.BrushShapeOverlay;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.V)]
internal class MoveToolViewModel : ToolViewModel, IMoveToolHandler
{
    private string defaultActionDisplay = "MOVE_TOOL_ACTION_DISPLAY";
    public override string ToolNameLocalizationKey => "MOVE_TOOL";

    private string transformingActionDisplay = "MOVE_TOOL_ACTION_DISPLAY_TRANSFORMING";
    private bool transformingSelectedArea = false;

    public bool MoveAllLayers { get; set; }

    public MoveToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = ToolbarFactory.Create(this);
        Cursor = new Cursor(StandardCursorType.Arrow);
    }

    public override LocalizedString Tooltip => new LocalizedString("MOVE_TOOL_TOOLTIP", Shortcut);

    [Settings.Bool("KEEP_ORIGINAL_IMAGE_SETTING")]
    public bool KeepOriginalImage => GetValue<bool>();

    public override BrushShape BrushShape => BrushShape.Hidden;
    public override bool HideHighlight => true;

    public bool TransformingSelectedArea
    {
        get => transformingSelectedArea;
        set
        {
            transformingSelectedArea = value;
            ActionDisplay = value ? transformingActionDisplay : defaultActionDisplay;
        }
    }

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseShiftLayerTool();
    }

    public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (TransformingSelectedArea)
        {
            return;
        }
        
        if (ctrlIsDown)
        {
            ActionDisplay = new LocalizedString("MOVE_TOOL_ACTION_DISPLAY_CTRL");
            MoveAllLayers = true;
        }
        else
        {
            ActionDisplay = defaultActionDisplay;
            MoveAllLayers = false;
        }
    }

    public override void OnSelected()
    {
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Operations.TransformSelectedArea(true);
    }
}
