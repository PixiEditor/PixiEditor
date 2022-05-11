using System;
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

    private bool isMoving = false;
    private Vector2d mousePosOnStartMove = new();
    private Vector2d originOnStartMove = new();
    private ShapeCorners cornersOnStartMove = new();

    private bool isRotating = false;
    private Vector2d mousePosOnStartRotate = new();
    private ShapeCorners cornersOnStartRotate = new();

    private Anchor? capturedAnchor;
    private bool originWasManuallyDragged = false;
    private ShapeCorners cornersOnStartAnchorDrag;
    private Vector2d mousePosOnStartAnchorDrag;
    private Vector2d originOnStartAnchorDrag;

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        TransformHelper.DrawOverlay(drawingContext, new(ActualWidth, ActualHeight), Corners, Origin, ZoomboxScale);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        e.Handled = true;
        var pos = TransformHelper.ToVector2d(e.GetPosition(this));
        var anchor = TransformHelper.GetAnchorInPosition(pos, Corners, Origin, ZoomboxScale);
        if (anchor is not null)
        {
            capturedAnchor = anchor;
            cornersOnStartAnchorDrag = Corners;
            originOnStartAnchorDrag = Origin;
            mousePosOnStartAnchorDrag = pos;
        }
        else if (Corners.IsPointInside(pos) || TransformHelper.IsWithinTransformHandle(TransformHelper.GetDragHandlePos(Corners, ZoomboxScale), pos, ZoomboxScale))
        {
            isMoving = true;
            mousePosOnStartMove = TransformHelper.ToVector2d(e.GetPosition(this));
            originOnStartMove = Origin;
            cornersOnStartMove = Corners;
        }
        else
        {
            isRotating = true;
            mousePosOnStartRotate = TransformHelper.ToVector2d(e.GetPosition(this));
            cornersOnStartRotate = Corners;
        }
        CaptureMouse();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (capturedAnchor is not null)
        {
            HandleCapturedAnchorMovement(e);
            return;
        }
        else if (isMoving)
        {
            var pos = TransformHelper.ToVector2d(e.GetPosition(this));
            var delta = pos - mousePosOnStartMove;
            Origin = originOnStartMove + delta;
            Corners = new ShapeCorners()
            {
                BottomLeft = cornersOnStartMove.BottomLeft + delta,
                BottomRight = cornersOnStartMove.BottomRight + delta,
                TopLeft = cornersOnStartMove.TopLeft + delta,
                TopRight = cornersOnStartMove.TopRight + delta,
            };
        }
        else if (isRotating)
        {
            var pos = TransformHelper.ToVector2d(e.GetPosition(this));
            var angle = (mousePosOnStartRotate - Origin).CCWAngleTo(pos - Origin);
            Corners = TransformUpdateHelper.UpdateShapeFromRotation(cornersOnStartRotate, Origin, angle);
        }
    }

    private void HandleCapturedAnchorMovement(MouseEventArgs e)
    {
        if (capturedAnchor is null)
            throw new InvalidOperationException("No anchor is captured");
        e.Handled = true;
        if (TransformHelper.IsCorner((Anchor)capturedAnchor) && CornerFreedom == TransformCornerFreedom.Locked ||
            TransformHelper.IsSide((Anchor)capturedAnchor) && SideFreedom == TransformSideFreedom.Locked)
            return;

        var pos = TransformHelper.ToVector2d(e.GetPosition(this));

        if (TransformHelper.IsCorner((Anchor)capturedAnchor))
        {
            var newCorners = TransformUpdateHelper.UpdateShapeFromCorner((Anchor)capturedAnchor, CornerFreedom, cornersOnStartAnchorDrag, pos - mousePosOnStartAnchorDrag);
            if (newCorners is not null)
                Corners = (ShapeCorners)newCorners;
            if (!originWasManuallyDragged)
                Origin = TransformHelper.OriginFromCorners(Corners);
        }
        else if (TransformHelper.IsSide((Anchor)capturedAnchor))
        {
            var newCorners = TransformUpdateHelper.UpdateShapeFromSide((Anchor)capturedAnchor, SideFreedom, cornersOnStartAnchorDrag, pos - mousePosOnStartAnchorDrag);
            if (newCorners is not null)
                Corners = (ShapeCorners)newCorners;
            if (!originWasManuallyDragged)
                Origin = TransformHelper.OriginFromCorners(Corners);
        }
        else if (capturedAnchor == Anchor.Origin)
        {
            originWasManuallyDragged = true;
            Origin = originOnStartAnchorDrag + pos - mousePosOnStartAnchorDrag;
        }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        if (ReleaseAnchor())
            e.Handled = true;
        else if (isMoving)
        {
            isMoving = false;
            e.Handled = true;
            ReleaseMouseCapture();
        }
        else if (isRotating)
        {
            isRotating = false;
            e.Handled = true;
            ReleaseMouseCapture();
        }
    }

    private static void OnRequestedCorners(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        TransformOverlay overlay = (TransformOverlay)obj;
        overlay.originWasManuallyDragged = false;
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
