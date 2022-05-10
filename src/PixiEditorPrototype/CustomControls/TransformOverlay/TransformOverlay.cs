using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.CustomControls.TransformOverlay;

internal class TransformOverlay : Control
{
    public static DependencyProperty RequestedCornersProperty =
        DependencyProperty.Register(nameof(RequestedCorners), typeof(ShapeCorners), typeof(TransformOverlay),
            new FrameworkPropertyMetadata(default(ShapeCorners), FrameworkPropertyMetadataOptions.AffectsRender, new(OnRequestedCorners)));

    public static DependencyProperty CornersProperty =
        DependencyProperty.Register(nameof(Corners), typeof(ShapeCorners), typeof(TransformOverlay),
            new FrameworkPropertyMetadata(default(ShapeCorners), FrameworkPropertyMetadataOptions.AffectsRender));

    public static DependencyProperty OriginProperty =
        DependencyProperty.Register(nameof(Origin), typeof(Vector2d), typeof(TransformOverlay),
            new FrameworkPropertyMetadata(default(Vector2d), FrameworkPropertyMetadataOptions.AffectsRender));

    public static DependencyProperty ZoomboxScaleProperty =
        DependencyProperty.Register(nameof(ZoomboxScale), typeof(double), typeof(TransformOverlay),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SideFreedomProperty =
        DependencyProperty.Register(nameof(SideFreedom), typeof(TransformSideFreedom), typeof(TransformOverlay),
            new FrameworkPropertyMetadata(TransformSideFreedom.Locked));

    public static readonly DependencyProperty CornerFreedomProperty =
        DependencyProperty.Register(nameof(CornerFreedom), typeof(TransformCornerFreedom), typeof(TransformOverlay),
            new FrameworkPropertyMetadata(TransformCornerFreedom.Locked));

    public TransformCornerFreedom CornerFreedom
    {
        get { return (TransformCornerFreedom)GetValue(CornerFreedomProperty); }
        set { SetValue(CornerFreedomProperty, value); }
    }

    public TransformSideFreedom SideFreedom
    {
        get => (TransformSideFreedom)GetValue(SideFreedomProperty);
        set => SetValue(SideFreedomProperty, value);
    }
    public ShapeCorners Corners
    {
        get => (ShapeCorners)GetValue(CornersProperty);
        set => SetValue(CornersProperty, value);
    }

    public ShapeCorners RequestedCorners
    {
        get => (ShapeCorners)GetValue(RequestedCornersProperty);
        set => SetValue(RequestedCornersProperty, value);
    }

    public double ZoomboxScale
    {
        get => (double)GetValue(ZoomboxScaleProperty);
        set => SetValue(ZoomboxScaleProperty, value);
    }
    public Vector2d Origin
    {
        get => (Vector2d)GetValue(OriginProperty);
        set => SetValue(OriginProperty, value);
    }

    private Anchor? capturedAnchor;

    private bool originMoved = false;
    private ShapeCorners mouseDownCorners;
    private Vector2d mouseDownOriginPos;

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        TransformHelper.DrawOverlay(drawingContext, Corners, Origin, ZoomboxScale);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        var pos = TransformHelper.ToVector2d(e.GetPosition(this));
        var anchor = TransformHelper.GetAnchorInPosition(pos, Corners, Origin, ZoomboxScale);
        if (anchor is null)
            return;
        capturedAnchor = anchor;

        mouseDownCorners = Corners;
        mouseDownOriginPos = Origin;

        e.Handled = true;
        CaptureMouse();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (capturedAnchor is null)
            return;
        e.Handled = true;
        if (TransformHelper.IsCorner((Anchor)capturedAnchor) && CornerFreedom == TransformCornerFreedom.Locked ||
            TransformHelper.IsSide((Anchor)capturedAnchor) && SideFreedom == TransformSideFreedom.Locked)
            return;

        var pos = TransformHelper.ToVector2d(e.GetPosition(this));

        if (TransformHelper.IsCorner((Anchor)capturedAnchor))
        {
            var newCorners = TransformUpdateHelper.UpdateShapeFromCorner((Anchor)capturedAnchor, CornerFreedom, mouseDownCorners, pos);
            if (newCorners is not null)
                Corners = (ShapeCorners)newCorners;
            if (!originMoved)
                Origin = TransformHelper.OriginFromCorners(Corners);
        }
        else if (TransformHelper.IsSide((Anchor)capturedAnchor))
        {
            var newCorners = TransformUpdateHelper.UpdateShapeFromSide((Anchor)capturedAnchor, SideFreedom, mouseDownCorners, pos);
            if (newCorners is not null)
                Corners = (ShapeCorners)newCorners;
            if (!originMoved)
                Origin = TransformHelper.OriginFromCorners(Corners);
        }
        else if (capturedAnchor == Anchor.Rotation)
        {
            var cur = TransformHelper.GetRotPos(mouseDownCorners, ZoomboxScale);
            var angle = (cur - mouseDownOriginPos).CCWAngleTo(pos - mouseDownOriginPos);
            Corners = TransformUpdateHelper.UpdateShapeFromRotation(mouseDownCorners, mouseDownOriginPos, angle);
        }
        else if (capturedAnchor == Anchor.Origin)
        {
            originMoved = true;
            Origin = pos;
        }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        if (ReleaseAnchor())
            e.Handled = true;
    }

    private static void OnRequestedCorners(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        TransformOverlay overlay = (TransformOverlay)obj;
        overlay.originMoved = false;
        overlay.Corners = (ShapeCorners)args.NewValue;
        overlay.Origin = TransformHelper.OriginFromCorners(overlay.Corners);
    }

    private bool ReleaseAnchor()
    {
        if (capturedAnchor is null)
            return false;
        ReleaseMouseCapture();
        capturedAnchor = null;
        return true;
    }
}
