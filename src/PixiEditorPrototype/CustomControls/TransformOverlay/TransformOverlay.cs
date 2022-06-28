using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.CustomControls.TransformOverlay;

internal class TransformOverlay : Control
{
    public static readonly DependencyProperty RequestedCornersProperty =
        DependencyProperty.Register(nameof(RequestedCorners), typeof(ShapeCorners), typeof(TransformOverlay),
            new FrameworkPropertyMetadata(default(ShapeCorners), FrameworkPropertyMetadataOptions.AffectsRender, OnRequestedCorners));

    public static readonly DependencyProperty CornersProperty =
        DependencyProperty.Register(nameof(Corners), typeof(ShapeCorners), typeof(TransformOverlay),
            new FrameworkPropertyMetadata(default(ShapeCorners), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ZoomboxScaleProperty =
        DependencyProperty.Register(nameof(ZoomboxScale), typeof(double), typeof(TransformOverlay),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SideFreedomProperty =
        DependencyProperty.Register(nameof(SideFreedom), typeof(TransformSideFreedom), typeof(TransformOverlay),
            new PropertyMetadata(TransformSideFreedom.Locked));

    public static readonly DependencyProperty CornerFreedomProperty =
        DependencyProperty.Register(nameof(CornerFreedom), typeof(TransformCornerFreedom), typeof(TransformOverlay),
            new PropertyMetadata(TransformCornerFreedom.Locked));

    public static readonly DependencyProperty SnapToAnglesProperty =
        DependencyProperty.Register(nameof(SnapToAngles), typeof(bool), typeof(TransformOverlay), new PropertyMetadata(false));

    public static readonly DependencyProperty InternalStateProperty =
        DependencyProperty.Register(nameof(InternalState), typeof(TransformState), typeof(TransformOverlay),
            new FrameworkPropertyMetadata(default(TransformState), FrameworkPropertyMetadataOptions.AffectsRender));

    public TransformState InternalState
    {
        get => (TransformState)GetValue(InternalStateProperty);
        set => SetValue(InternalStateProperty, value);
    }

    public bool SnapToAngles
    {
        get => (bool)GetValue(SnapToAnglesProperty);
        set => SetValue(SnapToAnglesProperty, value);
    }

    public TransformCornerFreedom CornerFreedom
    {
        get => (TransformCornerFreedom)GetValue(CornerFreedomProperty);
        set => SetValue(CornerFreedomProperty, value);
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

    private bool isResettingRequestedCorners = false;
    private bool isMoving = false;
    private VecD mousePosOnStartMove = new();
    private VecD originOnStartMove = new();
    private ShapeCorners cornersOnStartMove = new();

    private bool isRotating = false;
    private VecD mousePosOnStartRotate = new();
    private ShapeCorners cornersOnStartRotate = new();
    private double propAngle1OnStartRotate = 0;
    private double propAngle2OnStartRotate = 0;

    private Anchor? capturedAnchor;
    private ShapeCorners cornersOnStartAnchorDrag;
    private VecD mousePosOnStartAnchorDrag;
    private VecD originOnStartAnchorDrag;

    private Pen blackPen = new Pen(Brushes.Black, 1);
    private Pen blackDashedPen = new Pen(Brushes.Black, 1) { DashStyle = new DashStyle(new double[] { 2, 4 }, 0) };
    private Pen whiteDashedPen = new Pen(Brushes.White, 1) { DashStyle = new DashStyle(new double[] { 2, 4 }, 2) };
    private Pen blackFreqDashedPen = new Pen(Brushes.Black, 1) { DashStyle = new DashStyle(new double[] { 2, 2 }, 0) };
    private Pen whiteFreqDashedPen = new Pen(Brushes.White, 1) { DashStyle = new DashStyle(new double[] { 2, 2 }, 2) };

    private PathGeometry handleGeometry = new()
    {
        FillRule = FillRule.Nonzero,
        Figures = (PathFigureCollection?)new PathFigureCollectionConverter()
            .ConvertFrom("M 0.50025839 0 0.4248062 0.12971572 0.34987079 0.25994821 h 0.1002584 V 0.45012906 H 0.25994831 V 0.34987066 L 0.12971577 0.42480604 0 0.5002582 0.12971577 0.57519373 0.25994831 0.65012926 V 0.5498709 H 0.45012919 V 0.74005175 H 0.34987079 L 0.42480619 0.87028439 0.50025839 1 0.57519399 0.87028439 0.65012959 0.74005175 H 0.54987119 V 0.5498709 H 0.74005211 V 0.65012926 L 0.87028423 0.57519358 1 0.5002582 0.87028423 0.42480604 0.74005169 0.34987066 v 0.1002584 H 0.54987077 V 0.25994821 h 0.1002584 L 0.5751938 0.12971572 Z"),
    };

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        DrawOverlay(drawingContext, new(ActualWidth, ActualHeight), Corners, InternalState.Origin, ZoomboxScale);
    }

    private void DrawOverlay
        (DrawingContext context, VecD size, ShapeCorners corners, VecD origin, double zoomboxScale)
    {
        // draw transparent background to enable mouse input everywhere
        context.DrawRectangle(Brushes.Transparent, null, new Rect(new Point(0, 0), new Size(size.X, size.Y)));

        blackPen.Thickness = 1 / zoomboxScale;
        blackDashedPen.Thickness = 1 / zoomboxScale;
        whiteDashedPen.Thickness = 1 / zoomboxScale;
        blackFreqDashedPen.Thickness = 1 / zoomboxScale;
        whiteFreqDashedPen.Thickness = 1 / zoomboxScale;

        VecD topLeft = corners.TopLeft;
        VecD topRight = corners.TopRight;
        VecD bottomLeft = corners.BottomLeft;
        VecD bottomRight = corners.BottomRight;

        // lines
        context.DrawLine(blackDashedPen, TransformHelper.ToPoint(topLeft), TransformHelper.ToPoint(topRight));
        context.DrawLine(whiteDashedPen, TransformHelper.ToPoint(topLeft), TransformHelper.ToPoint(topRight));
        context.DrawLine(blackDashedPen, TransformHelper.ToPoint(topLeft), TransformHelper.ToPoint(bottomLeft));
        context.DrawLine(whiteDashedPen, TransformHelper.ToPoint(topLeft), TransformHelper.ToPoint(bottomLeft));
        context.DrawLine(blackDashedPen, TransformHelper.ToPoint(bottomRight), TransformHelper.ToPoint(bottomLeft));
        context.DrawLine(whiteDashedPen, TransformHelper.ToPoint(bottomRight), TransformHelper.ToPoint(bottomLeft));
        context.DrawLine(blackDashedPen, TransformHelper.ToPoint(bottomRight), TransformHelper.ToPoint(topRight));
        context.DrawLine(whiteDashedPen, TransformHelper.ToPoint(bottomRight), TransformHelper.ToPoint(topRight));

        // corner anchors
        context.DrawRectangle(Brushes.White, blackPen, TransformHelper.ToAnchorRect(topLeft, zoomboxScale));
        context.DrawRectangle(Brushes.White, blackPen, TransformHelper.ToAnchorRect(topRight, zoomboxScale));
        context.DrawRectangle(Brushes.White, blackPen, TransformHelper.ToAnchorRect(bottomLeft, zoomboxScale));
        context.DrawRectangle(Brushes.White, blackPen, TransformHelper.ToAnchorRect(bottomRight, zoomboxScale));

        // side anchors
        context.DrawRectangle(Brushes.White, blackPen, TransformHelper.ToAnchorRect((topLeft - topRight) / 2 + topRight, zoomboxScale));
        context.DrawRectangle(Brushes.White, blackPen, TransformHelper.ToAnchorRect((topLeft - bottomLeft) / 2 + bottomLeft, zoomboxScale));
        context.DrawRectangle(Brushes.White, blackPen, TransformHelper.ToAnchorRect((bottomLeft - bottomRight) / 2 + bottomRight, zoomboxScale));
        context.DrawRectangle(Brushes.White, blackPen, TransformHelper.ToAnchorRect((topRight - bottomRight) / 2 + bottomRight, zoomboxScale));

        // origin
        double radius = TransformHelper.AnchorSize / zoomboxScale / 2;
        context.DrawEllipse(Brushes.Transparent, blackFreqDashedPen, TransformHelper.ToPoint(origin), radius, radius);
        context.DrawEllipse(Brushes.Transparent, whiteFreqDashedPen, TransformHelper.ToPoint(origin), radius, radius);

        // move handle
        VecD handlePos = TransformHelper.GetDragHandlePos(corners, zoomboxScale);
        const double CrossSize = TransformHelper.MoveHandleSize - 1;
        context.DrawRectangle(Brushes.White, blackPen, TransformHelper.ToHandleRect(handlePos, zoomboxScale));
        handleGeometry.Transform = new MatrixTransform(
            0, CrossSize / zoomboxScale,
            CrossSize / zoomboxScale, 0,
            handlePos.X - CrossSize / (zoomboxScale * 2), handlePos.Y - CrossSize / (zoomboxScale * 2)
        );
        context.DrawGeometry(Brushes.Black, null, handleGeometry);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        e.Handled = true;
        var pos = TransformHelper.ToVecD(e.GetPosition(this));
        var anchor = TransformHelper.GetAnchorInPosition(pos, Corners, InternalState.Origin, ZoomboxScale);
        if (anchor is not null)
        {
            capturedAnchor = anchor;
            cornersOnStartAnchorDrag = Corners;
            originOnStartAnchorDrag = InternalState.Origin;
            mousePosOnStartAnchorDrag = pos;
        }
        else if (Corners.IsPointInside(pos) || TransformHelper.IsWithinTransformHandle(TransformHelper.GetDragHandlePos(Corners, ZoomboxScale), pos, ZoomboxScale))
        {
            isMoving = true;
            mousePosOnStartMove = TransformHelper.ToVecD(e.GetPosition(this));
            originOnStartMove = InternalState.Origin;
            cornersOnStartMove = Corners;
        }
        else
        {
            isRotating = true;
            mousePosOnStartRotate = TransformHelper.ToVecD(e.GetPosition(this));
            cornersOnStartRotate = Corners;
            propAngle1OnStartRotate = InternalState.ProportionalAngle1;
            propAngle2OnStartRotate = InternalState.ProportionalAngle2;
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

        if (isMoving)
        {
            var pos = TransformHelper.ToVecD(e.GetPosition(this));
            var delta = pos - mousePosOnStartMove;

            if (Corners.IsSnappedToPixels)
                delta = delta.Round();

            Corners = new ShapeCorners()
            {
                BottomLeft = cornersOnStartMove.BottomLeft + delta, BottomRight = cornersOnStartMove.BottomRight + delta, TopLeft = cornersOnStartMove.TopLeft + delta, TopRight = cornersOnStartMove.TopRight + delta,
            };

            InternalState = InternalState with { Origin = originOnStartMove + delta };
        }
        else if (isRotating)
        {
            var pos = TransformHelper.ToVecD(e.GetPosition(this));
            var angle = (mousePosOnStartRotate - InternalState.Origin).CCWAngleTo(pos - InternalState.Origin);
            if (SnapToAngles)
                angle = TransformHelper.FindSnappingAngle(cornersOnStartRotate, angle);
            InternalState = InternalState with { ProportionalAngle1 = propAngle1OnStartRotate + angle, ProportionalAngle2 = propAngle2OnStartRotate + angle, };
            Corners = TransformUpdateHelper.UpdateShapeFromRotation(cornersOnStartRotate, InternalState.Origin, angle);
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

        var pos = TransformHelper.ToVecD(e.GetPosition(this));

        if (TransformHelper.IsCorner((Anchor)capturedAnchor))
        {
            var targetPos = TransformHelper.GetAnchorPosition(cornersOnStartAnchorDrag, (Anchor)capturedAnchor) + pos - mousePosOnStartAnchorDrag;
            var newCorners = TransformUpdateHelper.UpdateShapeFromCorner
                ((Anchor)capturedAnchor, CornerFreedom, InternalState.ProportionalAngle1, InternalState.ProportionalAngle2, cornersOnStartAnchorDrag, targetPos);
            if (newCorners is not null)
            {
                bool shouldSnap = (CornerFreedom is TransformCornerFreedom.ScaleProportionally or TransformCornerFreedom.Scale) && Corners.IsSnappedToPixels;
                Corners = shouldSnap ? TransformHelper.SnapToPixels((ShapeCorners)newCorners) : (ShapeCorners)newCorners;
            }
            if (!InternalState.OriginWasManuallyDragged)
                InternalState = InternalState with { Origin = TransformHelper.OriginFromCorners(Corners) };
        }
        else if (TransformHelper.IsSide((Anchor)capturedAnchor))
        {
            var targetPos = TransformHelper.GetAnchorPosition(cornersOnStartAnchorDrag, (Anchor)capturedAnchor) + pos - mousePosOnStartAnchorDrag;
            var newCorners = TransformUpdateHelper.UpdateShapeFromSide
                ((Anchor)capturedAnchor, SideFreedom, InternalState.ProportionalAngle1, InternalState.ProportionalAngle2, cornersOnStartAnchorDrag, targetPos);
            if (newCorners is not null)
            {
                bool shouldSnap = (SideFreedom is TransformSideFreedom.ScaleProportionally or TransformSideFreedom.Stretch) && Corners.IsSnappedToPixels;
                Corners = shouldSnap ? TransformHelper.SnapToPixels((ShapeCorners)newCorners) : (ShapeCorners)newCorners;
            }
            if (!InternalState.OriginWasManuallyDragged)
                InternalState = InternalState with { Origin = TransformHelper.OriginFromCorners(Corners) };
        }
        else if (capturedAnchor == Anchor.Origin)
        {
            InternalState = InternalState with { OriginWasManuallyDragged = true, Origin = originOnStartAnchorDrag + pos - mousePosOnStartAnchorDrag, };
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
        if (overlay.isResettingRequestedCorners)
            return;
        overlay.Corners = (ShapeCorners)args.NewValue;
        overlay.InternalState = new()
        {
            ProportionalAngle1 = (overlay.Corners.BottomRight - overlay.Corners.TopLeft).Angle, ProportionalAngle2 = (overlay.Corners.TopRight - overlay.Corners.BottomLeft).Angle, OriginWasManuallyDragged = false, Origin = TransformHelper.OriginFromCorners(overlay.Corners),
        };
        overlay.isResettingRequestedCorners = true;
        overlay.RequestedCorners = new ShapeCorners();
        overlay.isResettingRequestedCorners = false;
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
