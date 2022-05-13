using System;
using System.ComponentModel;
using ChunkyImageLib.DataHolders;
using PixiEditorPrototype.CustomControls.TransformOverlay;

namespace PixiEditorPrototype.ViewModels;
internal class DocumentTransformViewModel : INotifyPropertyChanged
{
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

    public void HideTransform()
    {
        TransformActive = false;
    }

    public void ShowShapeTransform(ShapeCorners initPos)
    {
        CornerFreedom = TransformCornerFreedom.Scale;
        SideFreedom = TransformSideFreedom.ScaleProportionally;
        RequestedCorners = initPos;
        TransformActive = true;
    }

    public void ShowFreeTransform(ShapeCorners initPos)
    {
        CornerFreedom = TransformCornerFreedom.Free;
        SideFreedom = TransformSideFreedom.Free;
        RequestedCorners = initPos;
        TransformActive = true;
    }
}
