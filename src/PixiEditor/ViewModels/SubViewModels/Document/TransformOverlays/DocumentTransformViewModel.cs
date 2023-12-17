using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Helpers;
using PixiEditor.Helpers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Localization;
using PixiEditor.Views.UserControls.Overlays.TransformOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Document.TransformOverlays;
#nullable enable
[DebuggerDisplay("{ToString(),nq}")]
internal class DocumentTransformViewModel : NotifyableObject
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

    private bool lockRotation;
    public bool LockRotation
    {
        get => lockRotation;
        set => SetProperty(ref lockRotation, value);
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
                document.ActionDisplays[nameof(DocumentTransformViewModel)] = new LocalizedString($"TRANSFORM_ACTION_DISPLAY_{activeTransformMode.GetDescription()}");
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

    private bool coverWholeScreen;
    public bool CoverWholeScreen
    {
        get => coverWholeScreen;
        set => SetProperty(ref coverWholeScreen, value);
    }

    private ShapeCorners requestedCorners;
    public ShapeCorners RequestedCorners
    {
        get => requestedCorners;
        set
        {
            // The event must be raised even if the value hasn't changed, so I'm not using SetProperty
            requestedCorners = value;
            RaisePropertyChanged(nameof(RequestedCorners));
        }
    }

    private ShapeCorners corners;
    public ShapeCorners Corners
    {
        get => corners;
        set
        {
            SetProperty(ref corners, value);
            TransformMoved?.Invoke(this, value);
        }
    }

    private ICommand? actionCompletedCommand = null;
    public ICommand? ActionCompletedCommand
    {
        get => actionCompletedCommand;
        set => SetProperty(ref actionCompletedCommand, value);
    }

    public event EventHandler<ShapeCorners>? TransformMoved;

    private DocumentTransformMode activeTransformMode = DocumentTransformMode.Scale_Rotate_NoShear_NoPerspective;

    public DocumentTransformViewModel(DocumentViewModel document)
    {
        this.document = document;
        ActionCompletedCommand = new RelayCommand((_) =>
        {
            if (undoStack is null)
                return;

            var lastState = undoStack.PeekCurrent();
            if (lastState is not null && lastState.Value.Item1.AlmostEquals(Corners) && lastState.Value.Item2.AlmostEquals(InternalState))
                return;

            undoStack.AddState((Corners, InternalState), TransformOverlayStateType.Move);
        });
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
        if (undoStack is null)
            return false;

        InternalState = InternalState with { Origin = InternalState.Origin + distance };
        Corners = Corners.AsTranslated(distance);
        undoStack.AddState((Corners, InternalState), TransformOverlayStateType.Nudge);
        return true;
    }

    public void HideTransform()
    {
        if (undoStack is null)
            return;
        undoStack = null;

        TransformActive = false;
        ShowTransformControls = false;
    }

    public void ShowTransform(DocumentTransformMode mode, bool coverWholeScreen, ShapeCorners initPos, bool showApplyButton)
    {
        if (undoStack is not null)
            return;
        undoStack = new();

        activeTransformMode = mode;
        CornerFreedom = TransformCornerFreedom.Scale;
        SideFreedom = TransformSideFreedom.Stretch;
        LockRotation = mode == DocumentTransformMode.Scale_NoRotate_NoShear_NoPerspective;
        RequestedCorners = initPos;
        CoverWholeScreen = coverWholeScreen;
        TransformActive = true;
        ShowTransformControls = showApplyButton;

        undoStack.AddState((Corners, InternalState), TransformOverlayStateType.Initial);
    }

    public void ModifierKeysInlet(bool isShiftDown, bool isCtrlDown, bool isAltDown)
    {
        var requestedCornerFreedom = TransformCornerFreedom.Scale;
        var requestedSideFreedom = TransformSideFreedom.Stretch;

        SnapToAngles = isShiftDown;
        if (isShiftDown)
        {
            requestedCornerFreedom = TransformCornerFreedom.ScaleProportionally;
            requestedSideFreedom = TransformSideFreedom.ScaleProportionally;
        }
        else if (isCtrlDown)
        {
            requestedCornerFreedom = TransformCornerFreedom.Free;
            requestedSideFreedom = TransformSideFreedom.Free;
        }
        else if (isAltDown)
        {
            requestedSideFreedom = TransformSideFreedom.Shear;
        }
        else
        {
            requestedCornerFreedom = TransformCornerFreedom.Scale;
            requestedSideFreedom = TransformSideFreedom.Stretch;
        }

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

    public override string ToString() => !TransformActive ? "Not active" : $"Transform Mode: {activeTransformMode}; Corner Freedom: {CornerFreedom}; Side Freedom: {SideFreedom}";
}
