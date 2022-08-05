using System.ComponentModel;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Views.UserControls.TransformOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Document;
#nullable enable
internal class DocumentTransformViewModel : INotifyPropertyChanged
{
    private TransformState internalState;
    public TransformState InternalState
    {
        get => internalState;
        set
        {
            internalState = value;
            PropertyChanged?.Invoke(this, new(nameof(InternalState)));
        }
    }

    private TransformCornerFreedom cornerFreedom;
    public TransformCornerFreedom CornerFreedom
    {
        get => cornerFreedom;
        set
        {
            cornerFreedom = value;
            PropertyChanged?.Invoke(this, new(nameof(CornerFreedom)));
        }
    }

    private TransformSideFreedom sideFreedom;
    public TransformSideFreedom SideFreedom
    {
        get => sideFreedom;
        set
        {
            sideFreedom = value;
            PropertyChanged?.Invoke(this, new(nameof(SideFreedom)));
        }
    }

    private bool lockRotation;
    public bool LockRotation
    {
        get => lockRotation;
        set
        {
            lockRotation = value;
            PropertyChanged?.Invoke(this, new(nameof(LockRotation)));
        }
    }

    private bool transformActive;
    public bool TransformActive
    {
        get => transformActive;
        set
        {
            transformActive = value;
            PropertyChanged?.Invoke(this, new(nameof(TransformActive)));
        }
    }

    private ShapeCorners requestedCorners;
    public ShapeCorners RequestedCorners
    {
        get => requestedCorners;
        set
        {
            requestedCorners = value;
            PropertyChanged?.Invoke(this, new(nameof(RequestedCorners)));
        }
    }

    private ShapeCorners corners;
    public ShapeCorners Corners
    {
        get => corners;
        set
        {
            corners = value;
            PropertyChanged?.Invoke(this, new(nameof(Corners)));
            TransformMoved?.Invoke(this, value);
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
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
