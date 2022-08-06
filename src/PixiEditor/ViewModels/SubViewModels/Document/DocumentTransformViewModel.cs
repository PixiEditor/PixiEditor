using ChunkyImageLib.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Views.UserControls.TransformOverlay;

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

    private ShapeCorners requestedCorners;
    public ShapeCorners RequestedCorners
    {
        get => requestedCorners;
        set => SetProperty(ref requestedCorners, value);
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

    private DocumentTransformMode activeTransformMode = DocumentTransformMode.Rotation;

    public void HideTransform()
    {
        TransformActive = false;
    }

    public void ShowTransform(DocumentTransformMode mode, ShapeCorners initPos)
    {
        activeTransformMode = mode;
        CornerFreedom = TransformCornerFreedom.Scale;
        SideFreedom = TransformSideFreedom.Stretch;
        LockRotation = mode == DocumentTransformMode.NoRotation;
        RequestedCorners = initPos;
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

        if (requestedCornerFreedom != TransformCornerFreedom.Free || activeTransformMode == DocumentTransformMode.Freeform)
            CornerFreedom = requestedCornerFreedom;
        if (requestedSideFreedom is not (TransformSideFreedom.Free or TransformSideFreedom.Shear) ||
            activeTransformMode == DocumentTransformMode.Freeform)
            SideFreedom = requestedSideFreedom;
    }
}
