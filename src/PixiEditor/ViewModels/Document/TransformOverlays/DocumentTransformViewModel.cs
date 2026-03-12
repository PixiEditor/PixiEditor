using System.Diagnostics;
using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Extensions.Helpers;
using PixiEditor.Helpers.UI;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using Drawie.Numerics;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.UI.Common.Localization;
using PixiEditor.Views.Overlays.TransformOverlay;

namespace PixiEditor.ViewModels.Document.TransformOverlays;
#nullable enable
[DebuggerDisplay("{ToString(),nq}")]
internal class DocumentTransformViewModel : ObservableObject, ITransformHandler
{
    private DocumentViewModel document;

    private TransformOverlayUndoStack<(ShapeCorners, TransformState)>? undoStack = null;

    private TransformState internalState;

    public TransformState InternalState
    {
        get => internalState;
        set => SetProperty(ref internalState, value);
    }

    private TransformCornerFreedom cornerFreedom;

    public TransformCornerFreedom CornerFreedom
    {
        get => cornerFreedom;
        set => SetProperty(ref cornerFreedom, value);
    }

    private TransformSideFreedom sideFreedom;

    public TransformSideFreedom SideFreedom
    {
        get => sideFreedom;
        set => SetProperty(ref sideFreedom, value);
    }

    private bool scaleFromCenter;

    public bool ScaleFromCenter
    {
        get => scaleFromCenter;
        set => SetProperty(ref scaleFromCenter, value);
    }

    private bool lockRotation;

    public bool LockRotation
    {
        get => lockRotation;
        set => SetProperty(ref lockRotation, value);
    }

    private bool lockShear;

    public bool LockShear
    {
        get => lockShear;
        set => SetProperty(ref lockShear, value);
    }

    private bool snapToAngles;

    public bool SnapToAngles
    {
        get => snapToAngles;
        set => SetProperty(ref snapToAngles, value);
    }

    private bool transformActive;

    public bool TransformActive
    {
        get => transformActive;
        set
        {
            if (!SetProperty(ref transformActive, value))
            {
                return;
            }

            if (value)
            {
                document.ActionDisplays[nameof(DocumentTransformViewModel)] =
                    new LocalizedString($"TRANSFORM_ACTION_DISPLAY_{activeTransformMode.GetDescription()}");
            }
            else
            {
                document.ActionDisplays[nameof(DocumentTransformViewModel)] = null;
            }
        }
    }

    private bool showTransformControls;

    public bool ShowTransformControls
    {
        get => showTransformControls;
        set => SetProperty(ref showTransformControls, value);
    }

    private bool canAlignToPixels = true;

    public bool CanAlignToPixels
    {
        get => canAlignToPixels;
        set => SetProperty(ref canAlignToPixels, value);
    }

    public event Action<MouseOnCanvasEventArgs>? PassthroughPointerPressed;

    private bool coverWholeScreen;

    public bool CoverWholeScreen
    {
        get => coverWholeScreen;
        set => SetProperty(ref coverWholeScreen, value);
    }

    private ShapeCorners corners;

    public ShapeCorners Corners
    {
        get => corners;
        set
        {
            SetProperty(ref corners, value);
            TransformChanged?.Invoke(value);
        }
    }

    private bool showHandles;

    public bool ShowHandles
    {
        get => showHandles;
        set => SetProperty(ref showHandles, value);
    }

    private bool isSizeBoxEnabled;

    public bool IsSizeBoxEnabled
    {
        get => isSizeBoxEnabled;
        set => SetProperty(ref isSizeBoxEnabled, value);
    }


    private ExecutionTrigger<ShapeCorners> requestedCornersExecutor;

    public ExecutionTrigger<ShapeCorners> RequestCornersExecutor
    {
        get => requestedCornersExecutor;
        set => SetProperty(ref requestedCornersExecutor, value);
    }

    private ICommand? actionCompletedCommand = null;

    public ICommand? ActionCompletedCommand
    {
        get => actionCompletedCommand;
        set => SetProperty(ref actionCompletedCommand, value);
    }

    private ICommand? addToUndoCommand = null;

    public ICommand? AddToUndoCommand
    {
        get => addToUndoCommand;
        set => SetProperty(ref addToUndoCommand, value);
    }

    private RelayCommand<MouseOnCanvasEventArgs>? passThroughPointerPressedCommand;

    public RelayCommand<MouseOnCanvasEventArgs> PassThroughPointerPressedCommand
    {
        get
        {
            return passThroughPointerPressedCommand ??=
                new RelayCommand<MouseOnCanvasEventArgs>(x => PassthroughPointerPressed?.Invoke(x));
        }
    }

    private RelayCommand<(VecD, VecD)>? transformDraggedCommand;

    public RelayCommand<(VecD, VecD)> TransformDraggedCommand
    {
        get => transformDraggedCommand ??=
            new RelayCommand<(VecD from, VecD to)>(x => TransformDragged?.Invoke(x.from, x.to));
    }

    public event Action<ShapeCorners>? TransformChanged;
    public event Action<VecD, VecD> TransformDragged;
    public Action<bool> TransformShowStateChanged;
    public event Action TransformStopped;

    private DocumentTransformMode activeTransformMode = DocumentTransformMode.Scale_Rotate_NoShear_NoPerspective;

    public DocumentTransformViewModel(DocumentViewModel document)
    {
        this.document = document;
        ActionCompletedCommand = new RelayCommand(() =>
        {
            TransformStopped?.Invoke();
            AddToUndoCommand?.Execute(Corners);

            if (undoStack is null)
                return;

            var lastState = undoStack.PeekCurrent();
            if (lastState is not null && lastState.Value.Item1.AlmostEquals(Corners) &&
                lastState.Value.Item2.AlmostEquals(InternalState))
                return;

            undoStack.AddState((Corners, InternalState), TransformOverlayStateType.Move);
        });

        RequestCornersExecutor = new ExecutionTrigger<ShapeCorners>();
    }

    public bool Undo()
    {
        if (undoStack is null)
            return false;
        var state = undoStack.Undo();
        if (state is null)
            return false;
        (Corners, InternalState) = state.Value;
        return true;
    }

    public bool Redo()
    {
        if (undoStack is null)
            return false;
        var state = undoStack.Redo();
        if (state is null)
            return false;
        (Corners, InternalState) = state.Value;
        return true;
    }

    public bool Nudge(VecD distance)
    {
        InternalState = InternalState with { Origin = InternalState.Origin + distance };
        Corners = Corners.AsTranslated(distance);

        AddToUndoCommand?.Execute(Corners);

        undoStack?.AddState((Corners, InternalState), TransformOverlayStateType.Nudge);

        return true;
    }

    public bool HasUndo => undoStack is not null && undoStack.UndoCount > 0;
    public bool HasRedo => undoStack is not null && undoStack.RedoCount > 0;

    public void HideTransform()
    {
        undoStack = null;
        TransformActive = false;
        ShowTransformControls = false;
        TransformShowStateChanged?.Invoke(false);
    }

    public void ShowTransform(DocumentTransformMode mode, bool coverWholeScreen, ShapeCorners initPos,
        bool showApplyButton, Action<ShapeCorners>? customAddToUndo = null)
    {
        if (initPos.IsPartiallyDegenerate)
            return;

        activeTransformMode = mode;
        CornerFreedom = TransformCornerFreedom.Scale;
        SideFreedom = TransformSideFreedom.Stretch;
        LockRotation = mode == DocumentTransformMode.Scale_NoRotate_NoShear_NoPerspective;
        LockShear = mode is DocumentTransformMode.Scale_Rotate_NoShear_NoPerspective
            or DocumentTransformMode.Scale_NoRotate_NoShear_NoPerspective;
        CoverWholeScreen = coverWholeScreen;
        TransformActive = true;
        ShowTransformControls = showApplyButton;
        CanAlignToPixels = true;

        IsSizeBoxEnabled = false;
        ShowHandles = true;

        RequestCornersExecutor?.Execute(this, initPos);

        if (customAddToUndo is not null)
        {
            AddToUndoCommand = new RelayCommand<ShapeCorners>(customAddToUndo);
            undoStack = null;
        }
        else
        {
            undoStack = new TransformOverlayUndoStack<(ShapeCorners, TransformState)>();
            undoStack.AddState((Corners, InternalState), TransformOverlayStateType.Initial);
            AddToUndoCommand = null;
        }

        TransformShowStateChanged?.Invoke(true);
    }

    public void KeyModifiersInlet(bool isShiftDown, bool isCtrlDown, bool isAltDown)
    {
        var requestedCornerFreedom = TransformCornerFreedom.Scale;
        var requestedSideFreedom = TransformSideFreedom.Stretch;

        SnapToAngles = isShiftDown;
        if (isShiftDown)
        {
            requestedCornerFreedom = TransformCornerFreedom.ScaleProportionally;
            requestedSideFreedom = TransformSideFreedom.ScaleProportionally;
        }
        else if (isAltDown)
        {
            requestedCornerFreedom = TransformCornerFreedom.Free;
            requestedSideFreedom = TransformSideFreedom.Free;
        }
        else
        {
            requestedCornerFreedom = TransformCornerFreedom.Scale;
            requestedSideFreedom = TransformSideFreedom.Stretch;
        }

        ScaleFromCenter = isCtrlDown;

        UpdateFreedom(requestedCornerFreedom, requestedSideFreedom);
    }

    private void UpdateFreedom(TransformCornerFreedom requestedCornerFreedom, TransformSideFreedom requestedSideFreedom)
    {
        switch (activeTransformMode)
        {
            case DocumentTransformMode.Scale_Rotate_Shear_Perspective:
                CornerFreedom = requestedCornerFreedom;
                SideFreedom = requestedSideFreedom;
                break;

            case DocumentTransformMode.Scale_Rotate_Shear_NoPerspective:
                if (requestedCornerFreedom != TransformCornerFreedom.Free)
                    CornerFreedom = requestedCornerFreedom;
                SideFreedom = requestedSideFreedom;
                break;

            case DocumentTransformMode.Scale_Rotate_NoShear_NoPerspective:
            case DocumentTransformMode.Scale_NoRotate_NoShear_NoPerspective:
                if (requestedCornerFreedom != TransformCornerFreedom.Free)
                    CornerFreedom = requestedCornerFreedom;
                if (requestedSideFreedom is not (TransformSideFreedom.Free or TransformSideFreedom.Shear))
                    SideFreedom = requestedSideFreedom;
                break;
        }
    }

    public override string ToString() => !TransformActive
        ? "Not active"
        : $"Transform Mode: {activeTransformMode}; Corner Freedom: {CornerFreedom}; Side Freedom: {SideFreedom}";
}
