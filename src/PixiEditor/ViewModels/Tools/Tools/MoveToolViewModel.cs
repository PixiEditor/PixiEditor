using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers.Tools;
using Drawie.Numerics;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
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

    [Settings.Bool("_duplicate_on_move", ExposedByDefault = false)]
    public bool DuplicateOnMove
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }

    [Settings.Bool("_bilinear_transform", false, ExposedByDefault = false)]
    public bool BilinearTransform
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }

    public override BrushShape FinalBrushShape => BrushShape.Hidden;
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

    public override void KeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown, Key argsKey)
    {
        DuplicateOnMove = ctrlIsDown && argsKey is Key.None or Key.LeftCtrl or Key.RightCtrl && !shiftIsDown &&
                          !altIsDown;
    }

    protected override void OnSelected(bool restoring)
    {
        if (TransformingSelectedArea || restoring)
        {
            return;
        }

        DuplicateOnMove = false;
        var activeDoc = ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument;

        activeDoc?.Operations.TransformSelectedArea(true);
    }

    protected override void OnDeselecting(bool transient)
    {
        var vm = ViewModelMain.Current;
        if (!transient)
        {
            vm.DocumentManagerSubViewModel.ActiveDocument?.Operations.TryStopToolLinkedExecutor();
            TransformingSelectedArea = false;
            DuplicateOnMove = false;
        }
    }

    public override void OnPostUndoInlet()
    {
        if (IsActive)
        {
            OnToolSelected(false);
        }
    }

    public override void OnPostRedoInlet()
    {
        if (IsActive)
        {
            TransformingSelectedArea = false;
            OnToolSelected(false);
        }
    }

    public override void OnPreUndoInlet()
    {
        DuplicateOnMove = false;
    }

    public override void OnPreRedoInlet()
    {
        DuplicateOnMove = false;
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
