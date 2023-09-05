using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ChunkyImageLib.DataHolders;
using Hardware.Info;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.AvaloniaUI.Views.Overlays.Handles;
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

    private TransformHandle moveHandle;
    private RectangleHandle topLeftHandle;
    private RectangleHandle topRightHandle;
    private RectangleHandle bottomLeftHandle;
    private RectangleHandle bottomRightHandle;
    private RectangleHandle topHandle;
    private RectangleHandle bottomHandle;
    private RectangleHandle leftHandle;
    private RectangleHandle rightHandle;
    private OriginAnchor originHandle;

    private Dictionary<Handle, Anchor> anchorMap = new();

    private Geometry rotateCursorGeometry = Handle.GetHandleGeometry("RotateHandle");

    private Point lastPointerPos;
    private IPointer? capturedPointer;

    public TransformOverlay()
    {
        moveHandle = new TransformHandle(this, VecD.Zero);
        moveHandle.OnDrag += MoveHandleOnDrag;
        moveHandle.OnRelease += OnMoveHandleReleased;

        topLeftHandle = new AnchorHandle(this, VecD.Zero);
        topRightHandle = new AnchorHandle(this, VecD.Zero);
        bottomLeftHandle = new AnchorHandle(this, VecD.Zero);
        bottomRightHandle = new AnchorHandle(this, VecD.Zero);
        topHandle = new AnchorHandle(this, VecD.Zero);
        bottomHandle = new AnchorHandle(this, VecD.Zero);
        leftHandle = new AnchorHandle(this, VecD.Zero);
        rightHandle = new AnchorHandle(this, VecD.Zero);

        originHandle = new(this, VecD.Zero)
        {
            HandlePen = blackFreqDashedPen, SecondaryHandlePen = whiteFreqDashedPen, HandleBrush = Brushes.Transparent
        };

        AddHandle(originHandle);
        AddHandle(moveHandle);
        AddHandle(topLeftHandle);
        AddHandle(topRightHandle);
        AddHandle(bottomLeftHandle);
        AddHandle(bottomRightHandle);
        AddHandle(topHandle);
        AddHandle(bottomHandle);
        AddHandle(leftHandle);
        AddHandle(rightHandle);

        anchorMap.Add(topLeftHandle, Anchor.TopLeft);
        anchorMap.Add(topRightHandle, Anchor.TopRight);
        anchorMap.Add(bottomLeftHandle, Anchor.BottomLeft);
        anchorMap.Add(bottomRightHandle, Anchor.BottomRight);
        anchorMap.Add(topHandle, Anchor.Top);
        anchorMap.Add(bottomHandle, Anchor.Bottom);
        anchorMap.Add(leftHandle, Anchor.Left);
        anchorMap.Add(rightHandle, Anchor.Right);
        anchorMap.Add(originHandle, Anchor.Origin);

        ForAllHandles<AnchorHandle>(x =>
        {
            x.OnPress += OnAnchorHandlePressed;
            x.OnRelease += OnAnchorHandleReleased;
        });

        originHandle.OnPress += OnAnchorHandlePressed;
        originHandle.OnRelease += OnAnchorHandleReleased;

        moveHandle.OnPress += OnMoveHandlePressed;
    }

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
        /*double ellipseSize = (TransformHelper.AnchorSize * anchorSizeMultiplierForRotation - 2) / (ZoomboxScale * 2);
        foreach (var point in points)
        {
            context.DrawEllipse(Brushes.Transparent, null, point, ellipseSize, ellipseSize);
        }*/
    }

    private void DrawOverlay
        (DrawingContext context, VecD size, ShapeCorners corners, VecD origin, double zoomboxScale)
    {
        // draw transparent background to enable mouse input
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
        VecD top = (topLeft + topRight) / 2;
        VecD bottom = (bottomLeft + bottomRight) / 2;
        VecD left = (topLeft + bottomLeft) / 2;
        VecD right = (topRight + bottomRight) / 2;

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

        topLeftHandle.Position = topLeft;
        topRightHandle.Position = topRight;
        bottomLeftHandle.Position = bottomLeft;
        bottomRightHandle.Position = bottomRight;
        topHandle.Position = top;
        bottomHandle.Position = bottom;
        leftHandle.Position = left;
        rightHandle.Position = right;
        originHandle.Position = origin;

        topLeftHandle.Draw(context);
        topRightHandle.Draw(context);
        bottomLeftHandle.Draw(context);
        bottomRightHandle.Draw(context);
        topHandle.Draw(context);
        bottomHandle.Draw(context);
        leftHandle.Draw(context);
        rightHandle.Draw(context);
        originHandle.Draw(context);

        // move handle
        VecD handlePos = TransformHelper.GetHandlePos(corners, zoomboxScale, moveHandle.Size);
        moveHandle.Position = handlePos;
        moveHandle.Draw(context);

        // rotate cursor
        context.DrawGeometry(Brushes.White, blackPen, rotateCursorGeometry);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        rotateCursorGeometry.Transform = new ScaleTransform(0, 0);
    }

    private void OnMoveHandlePressed(Handle source, VecD position)
    {
        isMoving = true;
        mousePosOnStartMove = position;
        originOnStartMove = InternalState.Origin;
        cornersOnStartMove = Corners;
    }

    private void OnAnchorHandlePressed(Handle source, VecD position)
    {
        capturedAnchor = anchorMap[source];
        cornersOnStartAnchorDrag = Corners;
        originOnStartAnchorDrag = InternalState.Origin;
        mousePosOnStartAnchorDrag = TransformHelper.ToVecD(lastPointerPos);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetMouseButton(this) != MouseButton.Left)
            return;

        VecD pos = TransformHelper.ToVecD(e.GetPosition(this));
        if (!CanRotate(pos))
        {
            isMoving = true;
            mousePosOnStartMove = pos;
            originOnStartMove = InternalState.Origin;
            cornersOnStartMove = Corners;
        }
        else if (!LockRotation)
        {
            isRotating = true;
            mousePosOnStartRotate = pos;
            cornersOnStartRotate = Corners;
            propAngle1OnStartRotate = InternalState.ProportionalAngle1;
            propAngle2OnStartRotate = InternalState.ProportionalAngle2;
        }
        else
        {
            return;
        }
        
        e.Pointer.Capture(this);
        capturedPointer = e.Pointer;
    }

    private bool CanRotate(VecD mousePos)
    {
        return !Corners.IsPointInside(mousePos) && Handles.All(x => !x.IsWithinHandle(x.Position, mousePos, ZoomboxScale));
    }

    private bool UpdateRotationCursor(VecD mousePos)
    {
        if ((!CanRotate(mousePos) && !isRotating) || LockRotation)
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

    private void MoveHandleOnDrag(Handle source, VecD position)
    {
        VecD delta = position - mousePosOnStartMove;

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
        Anchor? anchor = TransformHelper.GetAnchorInPosition(pos, Corners, InternalState.Origin, ZoomboxScale, topLeftHandle.Size);

        if (isRotating)
        {
            finalCursor = new Cursor(StandardCursorType.None);
            double angle = (mousePosOnStartRotate - InternalState.Origin).CCWAngleTo(pos - InternalState.Origin);
            if (SnapToAngles)
                angle = TransformHelper.FindSnappingAngle(cornersOnStartRotate, angle);
            InternalState = InternalState with { ProportionalAngle1 = propAngle1OnStartRotate + angle, ProportionalAngle2 = propAngle2OnStartRotate + angle, };
            Corners = TransformUpdateHelper.UpdateShapeFromRotation(cornersOnStartRotate, InternalState.Origin, angle);
        }
        else if (anchor is not null)
        {
            if ((TransformHelper.IsCorner((Anchor)anchor) && CornerFreedom == TransformCornerFreedom.Free) ||
                (TransformHelper.IsSide((Anchor)anchor) && SideFreedom == TransformSideFreedom.Free))
                finalCursor = new Cursor(StandardCursorType.Arrow);
            else
                finalCursor = TransformHelper.GetResizeCursor((Anchor)anchor, Corners, ZoomboxAngle);
        }

        if (Cursor != finalCursor)
            Cursor = finalCursor;

        InvalidateVisual();
    }

    private void HandleCapturedAnchorMovement(PointerEventArgs e)
    {
        if (capturedAnchor is null)
            throw new InvalidOperationException("No anchor is captured");

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

    private void OnAnchorHandleReleased(Handle source)
    {
        capturedPointer = null;
        capturedAnchor = null;

        if (ActionCompleted is not null && ActionCompleted.CanExecute(null))
            ActionCompleted.Execute(null);
    }

    private void OnMoveHandleReleased(Handle source)
    {
        isMoving = false;
        capturedPointer = null;

        if (ActionCompleted is not null && ActionCompleted.CanExecute(null))
            ActionCompleted.Execute(null);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left)
            return;

        if (isRotating)
        {
            isRotating = false;
            e.Pointer.Capture(null);
            capturedPointer = null;
            Cursor = new Cursor(StandardCursorType.Arrow);
            var pos = TransformHelper.ToVecD(e.GetPosition(this));
            UpdateRotationCursor(pos);

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
}
