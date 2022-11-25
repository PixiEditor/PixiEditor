using ChunkyImageLib.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Views.UserControls.Overlays.TransformOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Document;
#nullable enable
internal class DocumentTransformViewModel : NotifyableObject
{
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
        set => SetProperty(ref transformActive, value);
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

    public event EventHandler<ShapeCorners>? TransformMoved;

    private DocumentTransformMode activeTransformMode = DocumentTransformMode.Scale_Rotate_NoShear_NoPerspective;

    public void HideTransform()
    {
        TransformActive = false;
    }

    public void ShowTransform(DocumentTransformMode mode, bool coverWholeScreen, ShapeCorners initPos)
    {
        activeTransformMode = mode;
        CornerFreedom = TransformCornerFreedom.Scale;
        SideFreedom = TransformSideFreedom.Stretch;
        LockRotation = mode == DocumentTransformMode.Scale_NoRotate_NoShear_NoPerspective;
        RequestedCorners = initPos;
        CoverWholeScreen = coverWholeScreen;
        TransformActive = true;
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
}
