using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ChunkyImageLib.DataHolders;
using Hardware.Info;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Overlays.TransformOverlay;
#nullable enable
internal class TransformOverlay : Overlay
{
    public static readonly StyledProperty<ShapeCorners> RequestedCornersProperty =
        AvaloniaProperty.Register<TransformOverlay, ShapeCorners>(nameof(RequestedCorners), defaultValue: default(ShapeCorners));

    public ShapeCorners RequestedCorners
    {
        get => GetValue(RequestedCornersProperty);
        set => SetValue(RequestedCornersProperty, value);
    }

    public static readonly StyledProperty<ShapeCorners> CornersProperty =
        AvaloniaProperty.Register<TransformOverlay, ShapeCorners>(nameof(Corners), defaultValue: default(ShapeCorners));

    public ShapeCorners Corners
    {
        get => GetValue(CornersProperty);
        set => SetValue(CornersProperty, value);
    }

    public static readonly StyledProperty<double> ZoomboxScaleProperty =
        AvaloniaProperty.Register<TransformOverlay, double>(nameof(ZoomboxScale), defaultValue: 1.0);

    public double ZoomboxScale
    {
        get => GetValue(ZoomboxScaleProperty);
        set => SetValue(ZoomboxScaleProperty, value);
    }

    public static readonly StyledProperty<TransformSideFreedom> SideFreedomProperty =
        AvaloniaProperty.Register<TransformOverlay, TransformSideFreedom>(nameof(SideFreedom), defaultValue: TransformSideFreedom.Locked);

    public TransformSideFreedom SideFreedom
    {
        get => GetValue(SideFreedomProperty);
        set => SetValue(SideFreedomProperty, value);
    }

    public static readonly StyledProperty<TransformCornerFreedom> CornerFreedomProperty =
        AvaloniaProperty.Register<TransformOverlay, TransformCornerFreedom>(nameof(CornerFreedom), defaultValue: TransformCornerFreedom.Locked);

    public TransformCornerFreedom CornerFreedom
    {
        get => GetValue(CornerFreedomProperty);
        set => SetValue(CornerFreedomProperty, value);
    }

    public static readonly StyledProperty<bool> LockRotationProperty =
        AvaloniaProperty.Register<TransformOverlay, bool>(nameof(LockRotation), defaultValue: false);

    public bool LockRotation
    {
        get => GetValue(LockRotationProperty);
        set => SetValue(LockRotationProperty, value);
    }

    public static readonly StyledProperty<bool> SnapToAnglesProperty =
        AvaloniaProperty.Register<TransformOverlay, bool>(nameof(SnapToAngles), defaultValue: false);

    public bool SnapToAngles
    {
        get => GetValue(SnapToAnglesProperty);
        set => SetValue(SnapToAnglesProperty, value);
    }

    public static readonly StyledProperty<TransformState> InternalStateProperty =
        AvaloniaProperty.Register<TransformOverlay, TransformState>(nameof(InternalState), defaultValue: default(TransformState));

    public TransformState InternalState
    {
        get => GetValue(InternalStateProperty);
        set => SetValue(InternalStateProperty, value);
    }

    public static readonly StyledProperty<double> ZoomboxAngleProperty =
        AvaloniaProperty.Register<TransformOverlay, double>(nameof(ZoomboxAngle), defaultValue: 0.0);

    public double ZoomboxAngle
    {
        get => GetValue(ZoomboxAngleProperty);
        set => SetValue(ZoomboxAngleProperty, value);
    }

    public static readonly StyledProperty<bool> CoverWholeScreenProperty =
        AvaloniaProperty.Register<TransformOverlay, bool>(nameof(CoverWholeScreen), defaultValue: true);

    public bool CoverWholeScreen
    {
        get => GetValue(CoverWholeScreenProperty);
        set => SetValue(CoverWholeScreenProperty, value);
    }

    public static readonly StyledProperty<ICommand?> ActionCompletedProperty =
        AvaloniaProperty.Register<TransformOverlay, ICommand?>(nameof(ActionCompleted));

    public ICommand? ActionCompleted
    {
        get => GetValue(ActionCompletedProperty);
        set => SetValue(ActionCompletedProperty, value);
    }

    static TransformOverlay()
    {
        AffectsRender<TransformOverlay>(RequestedCornersProperty, CornersProperty, ZoomboxScaleProperty, SideFreedomProperty, CornerFreedomProperty, LockRotationProperty, SnapToAnglesProperty, InternalStateProperty, ZoomboxAngleProperty, CoverWholeScreenProperty);

        RequestedCornersProperty.Changed.Subscribe(OnRequestedCorners);
    }

    private const int anchorSizeMultiplierForRotation = 15;

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

    private Geometry handleGeometry /*= GetHandleGeometry("MoveHandle")*/;

    private Geometry rotateCursorGeometry /*= GetHandleGeometry("RotateHandle")*/;

    private Point lastPointerPos;
    private IPointer? capturedPointer;


    public override void Render(DrawingContext drawingContext)
    {
        base.Render(drawingContext);
        DrawOverlay(drawingContext, new(Bounds.Width, Bounds.Height), Corners, InternalState.Origin, ZoomboxScale);

        if (capturedAnchor is null)
            UpdateRotationCursor(TransformHelper.ToVecD(lastPointerPos));
    }

    private void DrawMouseInputArea(DrawingContext context, VecD size)
    {
        if (CoverWholeScreen)
        {
            context.DrawRectangle(Brushes.Transparent, null, new Rect(new Point(-size.X * 50, -size.Y * 50), new Size(size.X * 101, size.Y * 101)));
            return;
        }

        StreamGeometry geometry = new();
        using (StreamGeometryContext ctx = geometry.Open())
        {
            ctx.BeginFigure(TransformHelper.ToPoint(Corners.TopLeft), true);
            ctx.LineTo(TransformHelper.ToPoint(Corners.TopRight));
            ctx.LineTo(TransformHelper.ToPoint(Corners.BottomRight));
            ctx.LineTo(TransformHelper.ToPoint(Corners.BottomLeft));
            ctx.EndFigure(true);
        }

        context.DrawGeometry(Brushes.Transparent, null, geometry);
        if (LockRotation)
            return;

        Span<Point> points = stackalloc Point[]
        {
            TransformHelper.ToPoint(Corners.TopLeft),
            TransformHelper.ToPoint(Corners.TopRight),
            TransformHelper.ToPoint(Corners.BottomLeft),
            TransformHelper.ToPoint(Corners.BottomRight),
            TransformHelper.ToPoint((Corners.TopLeft + Corners.TopRight) / 2),
            TransformHelper.ToPoint((Corners.TopLeft + Corners.BottomLeft) / 2),
            TransformHelper.ToPoint((Corners.BottomRight + Corners.TopRight) / 2),
            TransformHelper.ToPoint((Corners.BottomRight + Corners.BottomLeft) / 2),
        };
        double ellipseSize = (TransformHelper.AnchorSize * anchorSizeMultiplierForRotation - 2) / (ZoomboxScale * 2);
        foreach (var point in points)
        {
            context.DrawEllipse(Brushes.Transparent, null, point, ellipseSize, ellipseSize);
        }
    }

    private void DrawOverlay
        (DrawingContext context, VecD size, ShapeCorners corners, VecD origin, double zoomboxScale)
    {
        /*// draw transparent background to enable mouse input
        DrawMouseInputArea(context, size);

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
            new Matrix(
            0, CrossSize / zoomboxScale,
            CrossSize / zoomboxScale, 0,
            handlePos.X - CrossSize / (zoomboxScale * 2), handlePos.Y - CrossSize / (zoomboxScale * 2))
        );

        context.DrawGeometry(Brushes.Black, null, handleGeometry);

        // rotate cursor
        context.DrawGeometry(Brushes.White, blackPen, rotateCursorGeometry);*/
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        rotateCursorGeometry.Transform = new ScaleTransform(0, 0);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        /*base.OnPointerPressed(e);
        if (e.GetMouseButton(this) != MouseButton.Left)
            return;

        e.Handled = true;
        VecD pos = TransformHelper.ToVecD(e.GetPosition(this));
        Anchor? anchor = TransformHelper.GetAnchorInPosition(pos, Corners, InternalState.Origin, ZoomboxScale);
        if (anchor is not null)
        {
            capturedAnchor = anchor;
            cornersOnStartAnchorDrag = Corners;
            originOnStartAnchorDrag = InternalState.Origin;
            mousePosOnStartAnchorDrag = pos;
        }
        else if (!ShouldRotate(pos) || TransformHelper.IsWithinTransformHandle(TransformHelper.GetDragHandlePos(Corners, ZoomboxScale), pos, ZoomboxScale))
        {
            isMoving = true;
            mousePosOnStartMove = TransformHelper.ToVecD(e.GetPosition(this));
            originOnStartMove = InternalState.Origin;
            cornersOnStartMove = Corners;
        }
        else if (!LockRotation)
        {
            isRotating = true;
            mousePosOnStartRotate = TransformHelper.ToVecD(e.GetPosition(this));
            cornersOnStartRotate = Corners;
            propAngle1OnStartRotate = InternalState.ProportionalAngle1;
            propAngle2OnStartRotate = InternalState.ProportionalAngle2;
        }
        else
        {
            return;
        }
        
        e.Pointer.Capture(this);
        capturedPointer = e.Pointer;*/
    }

    private bool ShouldRotate(VecD mousePos)
    {
        return false;
        /*if (Corners.IsPointInside(mousePos) ||
            TransformHelper.GetAnchorInPosition(mousePos, Corners, InternalState.Origin, ZoomboxScale) is not null ||
            TransformHelper.IsWithinTransformHandle(TransformHelper.GetDragHandlePos(Corners, ZoomboxScale), mousePos, ZoomboxScale))
            return false;
        return TransformHelper.GetAnchorInPosition(mousePos, Corners, InternalState.Origin, ZoomboxScale, anchorSizeMultiplierForRotation) is not null;*/
    }

    private bool UpdateRotationCursor(VecD mousePos)
    {
        return false;
        if ((!ShouldRotate(mousePos) && !isRotating) || LockRotation)
        {
            rotateCursorGeometry.Transform = new ScaleTransform(0, 0);
            return false;
        }

        var matrix = new TranslateTransform(mousePos.X, mousePos.Y).Value;
        matrix = matrix.RotateAt((mousePos - InternalState.Origin).Angle * 180 / Math.PI - 90, mousePos.X, mousePos.Y);
        matrix = matrix.ScaleAt(8 / ZoomboxScale, 8 / ZoomboxScale, mousePos.X, mousePos.Y);
        rotateCursorGeometry.Transform = new MatrixTransform(matrix);
        return true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        Cursor finalCursor = new Cursor(StandardCursorType.Arrow);

        lastPointerPos = e.GetPosition(this);

        if (capturedAnchor is not null)
        {
            HandleCapturedAnchorMovement(e);
            return;
        }

        if (UpdateRotationCursor(TransformHelper.ToVecD(e.GetPosition(this))))
        {
            finalCursor = new Cursor(StandardCursorType.None);
        }

        VecD pos = TransformHelper.ToVecD(e.GetPosition(this));
        //Anchor? anchor = TransformHelper.GetAnchorInPosition(pos, Corners, InternalState.Origin, ZoomboxScale);

        if (isMoving)
        {
            finalCursor = new Cursor(StandardCursorType.SizeAll);
            VecD delta = pos - mousePosOnStartMove;

            if (Corners.IsSnappedToPixels)
                delta = delta.Round();

            Corners = new ShapeCorners()
            {
                BottomLeft = cornersOnStartMove.BottomLeft + delta,
                BottomRight = cornersOnStartMove.BottomRight + delta,
                TopLeft = cornersOnStartMove.TopLeft + delta,
                TopRight = cornersOnStartMove.TopRight + delta,
            };

            InternalState = InternalState with { Origin = originOnStartMove + delta };
        }
        else if (isRotating)
        {
            finalCursor = new Cursor(StandardCursorType.None);
            double angle = (mousePosOnStartRotate - InternalState.Origin).CCWAngleTo(pos - InternalState.Origin);
            if (SnapToAngles)
                angle = TransformHelper.FindSnappingAngle(cornersOnStartRotate, angle);
            InternalState = InternalState with { ProportionalAngle1 = propAngle1OnStartRotate + angle, ProportionalAngle2 = propAngle2OnStartRotate + angle, };
            Corners = TransformUpdateHelper.UpdateShapeFromRotation(cornersOnStartRotate, InternalState.Origin, angle);
        }
        /*else if (anchor is not null)
        {
            if ((TransformHelper.IsCorner((Anchor)anchor) && CornerFreedom == TransformCornerFreedom.Free) ||
                (TransformHelper.IsSide((Anchor)anchor) && SideFreedom == TransformSideFreedom.Free))
                finalCursor = new Cursor(StandardCursorType.Arrow);
            else
                finalCursor = TransformHelper.GetResizeCursor((Anchor)anchor, Corners, ZoomboxAngle);
        }*/

        if (Cursor != finalCursor)
            Cursor = finalCursor;

        InvalidateVisual();
    }

    private void HandleCapturedAnchorMovement(PointerEventArgs e)
    {
        if (capturedAnchor is null)
            throw new InvalidOperationException("No anchor is captured");
        e.Handled = true;
        if ((TransformHelper.IsCorner((Anchor)capturedAnchor) && CornerFreedom == TransformCornerFreedom.Locked) ||
            (TransformHelper.IsSide((Anchor)capturedAnchor) && SideFreedom == TransformSideFreedom.Locked))
            return;

        VecD pos = TransformHelper.ToVecD(e.GetPosition(this));

        if (TransformHelper.IsCorner((Anchor)capturedAnchor))
        {
            VecD targetPos = TransformHelper.GetAnchorPosition(cornersOnStartAnchorDrag, (Anchor)capturedAnchor) + pos - mousePosOnStartAnchorDrag;
            ShapeCorners? newCorners = TransformUpdateHelper.UpdateShapeFromCorner
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
            VecD targetPos = TransformHelper.GetAnchorPosition(cornersOnStartAnchorDrag, (Anchor)capturedAnchor) + pos - mousePosOnStartAnchorDrag;
            ShapeCorners? newCorners = TransformUpdateHelper.UpdateShapeFromSide
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

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left)
            return;

        bool handled = false;
        if (ReleaseAnchor())
        {
            handled = true;
        }
        else if (isMoving)
        {
            isMoving = false;
            handled = true;
            e.Pointer.Capture(null);
            capturedPointer = null;
        }
        else if (isRotating)
        {
            isRotating = false;
            handled = true;
            e.Pointer.Capture(null);
            capturedPointer = null;
            Cursor = new Cursor(StandardCursorType.Arrow);
            var pos = TransformHelper.ToVecD(e.GetPosition(this));
            UpdateRotationCursor(pos);
        }

        if (handled)
        {
            e.Handled = true;
            if (ActionCompleted is not null && ActionCompleted.CanExecute(null))
                ActionCompleted.Execute(null);
        }
    }

    private static void OnRequestedCorners(AvaloniaPropertyChangedEventArgs<ShapeCorners> args)
    {
        TransformOverlay overlay = (TransformOverlay)args.Sender;
        if (overlay.isResettingRequestedCorners)
            return;
        overlay.isMoving = false;
        overlay.isRotating = false;
        overlay.Corners = args.NewValue.Value;
        overlay.InternalState = new()
        {
            ProportionalAngle1 = (overlay.Corners.BottomRight - overlay.Corners.TopLeft).Angle,
            ProportionalAngle2 = (overlay.Corners.TopRight - overlay.Corners.BottomLeft).Angle,
            OriginWasManuallyDragged = false,
            Origin = TransformHelper.OriginFromCorners(overlay.Corners),
        };
        overlay.isResettingRequestedCorners = true;
        overlay.RequestedCorners = new ShapeCorners();
        overlay.isResettingRequestedCorners = false;
    }

    private bool ReleaseAnchor()
    {
        if (capturedAnchor is null)
            return false;
        capturedPointer?.Capture(null);
        capturedPointer = null;
        capturedAnchor = null;
        return true;
    }
}
