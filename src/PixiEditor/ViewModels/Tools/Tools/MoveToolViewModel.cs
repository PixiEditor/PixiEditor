using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces.Vector;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Numerics;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.V)]
internal class MoveToolViewModel : ToolViewModel, IMoveToolHandler
{
    private string defaultActionDisplay = "MOVE_TOOL_ACTION_DISPLAY";
    public override string ToolNameLocalizationKey => "MOVE_TOOL";

    private string transformingActionDisplay = "MOVE_TOOL_ACTION_DISPLAY_TRANSFORMING";
    private bool transformingSelectedArea = false;
    public bool MoveAllLayers { get; set; }

    public override string Icon => PixiPerfectIcons.MousePointer;

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
    public override Type[] SupportedLayerTypes { get; } = [];
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

    public override void OnDeselecting()
    {
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Operations.TryStopToolLinkedExecutor();
    }

    private static RectI? GetSelectedLayersBounds()
    {
        var layers = ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.ExtractSelectedLayers();
        RectI? bounds = null;
        if (layers != null)
        {
            foreach (var layer in layers)
            {
                var foundLayer =
                    ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument.StructureHelper.Find(layer);
                RectI? layerBounds = (RectI?)foundLayer?.TightBounds;
                if (layerBounds != null)
                {
                    if (bounds == null)
                    {
                        bounds = layerBounds;
                    }
                    else
                    {
                        bounds = bounds.Value.Union(layerBounds.Value);
                    }
                }
            }
        }

        return bounds;
    }
}
