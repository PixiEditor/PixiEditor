using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers.Tools;
using Drawie.Numerics;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.V, Transient = Key.Space)]
internal class MoveToolViewModel : ToolViewModel, IMoveToolHandler
{
    private string defaultActionDisplay = "MOVE_TOOL_ACTION_DISPLAY";
    public override string ToolNameLocalizationKey => "MOVE_TOOL";

    private string transformingActionDisplay = "MOVE_TOOL_ACTION_DISPLAY_TRANSFORMING";
    private bool transformingSelectedArea = false;

    public override string DefaultIcon => PixiPerfectIcons.MousePointer;

    public MoveToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = ToolbarFactory.Create(this);
        Cursor = new Cursor(StandardCursorType.Arrow);
    }

    public override LocalizedString Tooltip => new LocalizedString("MOVE_TOOL_TOOLTIP", Shortcut);

    [Settings.Bool("KEEP_ORIGINAL_IMAGE_SETTING", Notify = nameof(KeepOriginalChanged))]
    public bool KeepOriginalImage
    {
        get
        {
            return GetValue<bool>();
        }
    }

    public override BrushShape BrushShape => BrushShape.Hidden;
    public override Type[]? SupportedLayerTypes { get; } = null;
    public override Type LayerTypeToCreateOnEmptyUse { get; } = null;
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
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Operations.TransformSelectedArea(true);
    }

    public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        
    }

    protected override void OnSelected(bool restoring)
    {
        if (TransformingSelectedArea)
        {
            return;
        }
        
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Operations.TransformSelectedArea(true);
    }

    protected override void OnDeselecting(bool transient)
    {
        var vm = ViewModelMain.Current;
        if (!transient)
        {
            vm.DocumentManagerSubViewModel.ActiveDocument?.Operations.TryStopToolLinkedExecutor();
            TransformingSelectedArea = false;
        }
    }

    public override void OnPostUndo()
    {
        if (IsActive)
        {
           OnToolSelected(false);
        }
    }

    public override void OnPostRedo()
    {
        if (IsActive)
        {
            OnToolSelected(false);
        }
    }

    protected override void OnSelectedLayersChanged(IStructureMemberHandler[] layers)
    {
        UpdateSelection();
    }
    
    public override void OnActiveFrameChanged(int newFrame)
    {
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        OnToolDeselected(false);
        OnToolSelected(false);
    }

    public void KeepOriginalChanged()
    {
        var activeDocument = ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument;
        if (activeDocument is null)
        {
            return;
        }
        
        activeDocument.TransformViewModel.ShowTransformControls = KeepOriginalImage;
    }
}
