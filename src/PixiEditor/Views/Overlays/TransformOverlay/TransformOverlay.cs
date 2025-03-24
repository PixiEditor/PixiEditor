using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using PixiEditor.Extensions.UI.Overlays;
using PixiEditor.Helpers.UI;
using PixiEditor.Models.Controllers.InputDevice;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Views.Overlays.Drawables;
using PixiEditor.Views.Overlays.Handles;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace PixiEditor.Views.Overlays.TransformOverlay;
#nullable enable
internal class TransformOverlay : Overlay
{
    public static readonly StyledProperty<ShapeCorners> CornersProperty =
        AvaloniaProperty.Register<TransformOverlay, ShapeCorners>(nameof(Corners), defaultValue: default(ShapeCorners));

    public ShapeCorners Corners
    {
        get => GetValue(CornersProperty);
        set => SetValue(CornersProperty, value);
    }

    public static readonly StyledProperty<TransformSideFreedom> SideFreedomProperty =
        AvaloniaProperty.Register<TransformOverlay, TransformSideFreedom>(nameof(SideFreedom),
            defaultValue: TransformSideFreedom.Locked);

    public TransformSideFreedom SideFreedom
    {
        get => GetValue(SideFreedomProperty);
        set => SetValue(SideFreedomProperty, value);
    }

    public static readonly StyledProperty<TransformCornerFreedom> CornerFreedomProperty =
        AvaloniaProperty.Register<TransformOverlay, TransformCornerFreedom>(nameof(CornerFreedom),
            defaultValue: TransformCornerFreedom.Locked);

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
        AvaloniaProperty.Register<TransformOverlay, TransformState>(nameof(InternalState),
            defaultValue: default(TransformState));

    public static readonly StyledProperty<ICommand> PassthroughPointerPressedCommandProperty =
        AvaloniaProperty.Register<TransformOverlay, ICommand>(
            nameof(PassthroughPointerPressedCommand));

    public ICommand PassthroughPointerPressedCommand
    {
        get => GetValue(PassthroughPointerPressedCommandProperty);
        set => SetValue(PassthroughPointerPressedCommandProperty, value);
    }

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

    public static readonly StyledProperty<ExecutionTrigger<ShapeCorners>> RequestCornersExecutorProperty =
        AvaloniaProperty.Register<TransformOverlay, ExecutionTrigger<ShapeCorners>>(
            nameof(RequestCornersExecutor));

    public ExecutionTrigger<ShapeCorners> RequestCornersExecutor
    {
        get => GetValue(RequestCornersExecutorProperty);
        set => SetValue(RequestCornersExecutorProperty, value);
    }

    public static readonly StyledProperty<ICommand?> ActionCompletedProperty =
        AvaloniaProperty.Register<TransformOverlay, ICommand?>(nameof(ActionCompleted));

    public ICommand? ActionCompleted
    {
        get => GetValue(ActionCompletedProperty);
        set => SetValue(ActionCompletedProperty, value);
    }

    public static readonly StyledProperty<SnappingController> SnappingControllerProperty =
        AvaloniaProperty.Register<TransformOverlay, SnappingController>(
            nameof(SnappingController));

    public SnappingController SnappingController
    {
        get => GetValue(SnappingControllerProperty);
        set => SetValue(SnappingControllerProperty, value);
    }

    public static readonly StyledProperty<bool> ShowHandlesProperty = AvaloniaProperty.Register<TransformOverlay, bool>(
        nameof(ShowHandles));

    public bool ShowHandles
    {
        get => GetValue(ShowHandlesProperty);
        set => SetValue(ShowHandlesProperty, value);
    }

    public static readonly StyledProperty<bool> IsSizeBoxEnabledProperty =
        AvaloniaProperty.Register<TransformOverlay, bool>(
            nameof(IsSizeBoxEnabled));

    public bool IsSizeBoxEnabled
    {
        get => GetValue(IsSizeBoxEnabledProperty);
        set => SetValue(IsSizeBoxEnabledProperty, value);
    }

    public static readonly StyledProperty<bool> ScaleFromCenterProperty =
        AvaloniaProperty.Register<TransformOverlay, bool>(
            nameof(ScaleFromCenter));

    public bool ScaleFromCenter
    {
        get => GetValue(ScaleFromCenterProperty);
        set => SetValue(ScaleFromCenterProperty, value);
    }

    public static readonly StyledProperty<bool> CanAlignToPixelsProperty =
        AvaloniaProperty.Register<TransformOverlay, bool>(
            nameof(CanAlignToPixels), defaultValue: true);

    public bool CanAlignToPixels
    {
        get => GetValue(CanAlignToPixelsProperty);
        set => SetValue(CanAlignToPixelsProperty, value);
    }

    public static readonly StyledProperty<bool> LockShearProperty = AvaloniaProperty.Register<TransformOverlay, bool>(
        nameof(LockShear));

    public bool LockShear
    {
        get => GetValue(LockShearProperty);
        set => SetValue(LockShearProperty, value);
    }

    public static readonly StyledProperty<ICommand> TransformDraggedCommandProperty =
        AvaloniaProperty.Register<TransformOverlay, ICommand>(
            nameof(TransformDraggedCommand));

    public ICommand TransformDraggedCommand
    {
        get => GetValue(TransformDraggedCommandProperty);
        set => SetValue(TransformDraggedCommandProperty, value);
    }

    static TransformOverlay()
    {
        AffectsRender<TransformOverlay>(CornersProperty, ZoomScaleProperty, SideFreedomProperty, CornerFreedomProperty,
            LockRotationProperty, SnapToAnglesProperty, InternalStateProperty, ZoomboxAngleProperty,
            CoverWholeScreenProperty);
        RequestCornersExecutorProperty.Changed.Subscribe(OnCornersExecutorChanged);
    }

    private bool isMoving = false;
    private VecD mousePosOnStartMove = new();
    private VecD originOnStartMove = new();
    private ShapeCorners cornersOnStartMove = new();

    private bool isRotating = false;
    private VecD mousePosOnStartRotate = new();
    private ShapeCorners cornersOnStartRotate = new();
    private double propAngle1OnStartRotate = 0;
    private double propAngle2OnStartRotate = 0;

    private TransformSideFreedom beforeShearSideFreedom;
    private Anchor? capturedAnchor;
    private ShapeCorners cornersOnStartAnchorDrag;
    private VecD mousePosOnStartAnchorDrag;
    private VecD originOnStartAnchorDrag;

    private Paint handlePen = new Paint()
    {
        Color = Colors.Black, StrokeWidth = 1, Style = PaintStyle.Stroke, IsAntiAliased = true
    };

    private Paint cursorBorderPaint = new Paint()
    {
        Color = Colors.Black, StrokeWidth = 0.08f, Style = PaintStyle.Stroke, IsAntiAliased = true
    };

    private Paint whiteFillPen = new Paint()
    {
        Color = Colors.White, StrokeWidth = 1, Style = PaintStyle.Fill, IsAntiAliased = true
    };

    private Paint blackDashedPen = new Paint()
    {
        Color = Colors.Black,
        StrokeWidth = 1,
        Style = PaintStyle.Stroke,
        PathEffect = PathEffect.CreateDash(
            [2, 4], 0),
        IsAntiAliased = true
    };

    private Paint whiteDashedPen = new Paint()
    {
        Color = Colors.White,
        StrokeWidth = 1,
        Style = PaintStyle.Stroke,
        PathEffect = PathEffect.CreateDash([2, 4], 2),
        IsAntiAliased = true
    };

    private Paint blackFreqDashedPen = new Paint()
    {
        Color = Colors.Black,
        StrokeWidth = 1,
        Style = PaintStyle.Stroke,
        PathEffect = PathEffect.CreateDash([2, 2], 0),
        IsAntiAliased = true
    };

    private Paint whiteFreqDashedPen = new Paint()
    {
        Color = Colors.White,
        StrokeWidth = 1,
        Style = PaintStyle.Stroke,
        PathEffect = PathEffect.CreateDash([2, 2], 2),
        IsAntiAliased = true
    };

    private AnchorHandle topLeftHandle;
    private AnchorHandle topRightHandle;
    private AnchorHandle bottomLeftHandle;
    private AnchorHandle bottomRightHandle;
    private AnchorHandle topHandle;
    private AnchorHandle bottomHandle;
    private AnchorHandle leftHandle;
    private AnchorHandle rightHandle;
    private RectangleHandle centerHandle;
    private OriginAnchor originHandle;
    private TransformHandle moveHandle;

    private Dictionary<Handle, Anchor> anchorMap = new();
    private List<Handle> snapPoints = new();
    private Handle? snapHandleOfOrigin;

    private VectorPath rotateCursorGeometry = Handle.GetHandleGeometry("RotateHandle").Path;
    private PathVectorData shearCursorGeometry = Handle.GetHandleGeometry("ShearHandle");
    private bool rotationCursorActive = false;
    private bool shearCursorActive = false;
    private Anchor? hoveredAnchor;

    private VecD lastPointerPos;
    private InfoBox infoBox;
    private VecD lastSize;
    private bool actuallyMoved = false;
    private bool isShearing = false;
    private int lastClickCount = 0;
    private bool pressedWithinBounds = false;

    public TransformOverlay()
    {
        topLeftHandle = new AnchorHandle(this);
        topRightHandle = new AnchorHandle(this);
        bottomLeftHandle = new AnchorHandle(this);
        bottomRightHandle = new AnchorHandle(this);
        topHandle = new AnchorHandle(this);
        bottomHandle = new AnchorHandle(this);
        leftHandle = new AnchorHandle(this);
        rightHandle = new AnchorHandle(this);
        moveHandle = new(this);
        moveHandle.StrokePaint = handlePen;
        centerHandle = new RectangleHandle(this);
        centerHandle.Size = rightHandle.Size;
        centerHandle.HitTestVisible = false;

        originHandle = new(this) { StrokePaint = blackFreqDashedPen, SecondaryHandlePen = whiteFreqDashedPen, };

        AddHandle(originHandle);
        AddHandle(topLeftHandle);
        AddHandle(topRightHandle);
        AddHandle(bottomLeftHandle);
        AddHandle(bottomRightHandle);
        AddHandle(topHandle);
        AddHandle(bottomHandle);
        AddHandle(leftHandle);
        AddHandle(rightHandle);
        AddHandle(centerHandle);
        AddHandle(moveHandle);

        anchorMap.Add(topLeftHandle, Anchor.TopLeft);
        anchorMap.Add(topRightHandle, Anchor.TopRight);
        anchorMap.Add(bottomLeftHandle, Anchor.BottomLeft);
        anchorMap.Add(bottomRightHandle, Anchor.BottomRight);
        anchorMap.Add(topHandle, Anchor.Top);
        anchorMap.Add(bottomHandle, Anchor.Bottom);
        anchorMap.Add(leftHandle, Anchor.Left);
        anchorMap.Add(rightHandle, Anchor.Right);
        anchorMap.Add(originHandle, Anchor.Origin);

        ForAllHandles<RectangleHandle>(snapPoints.Add);

        ForAllHandles<AnchorHandle>(x =>
        {
            x.OnPress += OnAnchorHandlePressed;
            x.OnDrag += OnAnchorHandleDrag;
            x.OnRelease += OnAnchorHandleReleased;
        });

        originHandle.OnPress += OnAnchorHandlePressed;
        originHandle.OnDrag += OnAnchorHandleDrag;
        originHandle.OnRelease += OnAnchorHandleReleased;

        moveHandle.OnPress += OnMoveHandlePressed;
        moveHandle.OnDrag += OnMoveHandleDrag;
        moveHandle.OnRelease += OnMoveHandleReleased;

        infoBox = new InfoBox();
    }

    private VecD pos;

    public override void RenderOverlay(Canvas drawingContext, RectD canvasBounds)
    {
        DrawOverlay(drawingContext, canvasBounds.Size, Corners, InternalState.Origin, (float)ZoomScale);

        if (capturedAnchor is null)
        {
            UpdateSpecialCursors(lastPointerPos);
        }
    }

    private void DrawOverlay
        (Canvas context, VecD size, ShapeCorners corners, VecD origin, float zoomboxScale)
    {
        lastSize = size;

        handlePen.StrokeWidth = 1 / zoomboxScale;
        blackDashedPen.StrokeWidth = 1 / zoomboxScale;
        whiteDashedPen.StrokeWidth = 1 / zoomboxScale;
        blackFreqDashedPen.StrokeWidth = 1 / zoomboxScale;
        whiteFreqDashedPen.StrokeWidth = 1 / zoomboxScale;

        blackDashedPen.PathEffect?.Dispose();
        blackDashedPen.PathEffect = PathEffect.CreateDash([2 / zoomboxScale, 4 / zoomboxScale], 0);

        whiteDashedPen.PathEffect?.Dispose();
        whiteDashedPen.PathEffect = PathEffect.CreateDash([2 / zoomboxScale, 4 / zoomboxScale], 2);

        blackFreqDashedPen.PathEffect?.Dispose();
        blackFreqDashedPen.PathEffect = PathEffect.CreateDash([2 / zoomboxScale, 2 / zoomboxScale], 0);

        whiteFreqDashedPen.PathEffect?.Dispose();
        whiteFreqDashedPen.PathEffect = PathEffect.CreateDash([2 / zoomboxScale, 2 / zoomboxScale], 2);

        VecD topLeft = corners.TopLeft;
        VecD topRight = corners.TopRight;
        VecD bottomLeft = corners.BottomLeft;
        VecD bottomRight = corners.BottomRight;
        VecD top = (topLeft + topRight) / 2;
        VecD bottom = (bottomLeft + bottomRight) / 2;
        VecD left = (topLeft + bottomLeft) / 2;
        VecD right = (topRight + bottomRight) / 2;

        // lines
        context.DrawLine(topLeft, topRight, blackDashedPen);
        context.DrawLine(topLeft, topRight, whiteDashedPen);
        context.DrawLine(topLeft, bottomLeft, blackDashedPen);
        context.DrawLine(topLeft, bottomLeft, whiteDashedPen);
        context.DrawLine(bottomRight, bottomLeft, blackDashedPen);
        context.DrawLine(bottomRight, bottomLeft, whiteDashedPen);
        context.DrawLine(bottomRight, topRight, blackDashedPen);
        context.DrawLine(bottomRight, topRight, whiteDashedPen);

        // corner anchors

        if (ShowHandles)
        {
            centerHandle.Position = VecD.Zero;
            centerHandle.HitTestVisible = capturedAnchor == Anchor.Origin;
            topLeftHandle.Position = topLeft;
            topRightHandle.Position = topRight;
            bottomLeftHandle.Position = bottomLeft;
            bottomRightHandle.Position = bottomRight;
            topHandle.Position = top;
            bottomHandle.Position = bottom;
            leftHandle.Position = left;
            rightHandle.Position = right;
            originHandle.Position = InternalState.Origin;
            moveHandle.Position = TransformHelper.GetHandlePos(Corners, ZoomScale, moveHandle.Size);

            topLeftHandle.Draw(context);
            topRightHandle.Draw(context);
            bottomLeftHandle.Draw(context);
            bottomRightHandle.Draw(context);
            topHandle.Draw(context);
            bottomHandle.Draw(context);
            leftHandle.Draw(context);
            rightHandle.Draw(context);
            originHandle.Draw(context);
            moveHandle.Draw(context);

            if (capturedAnchor == Anchor.Origin)
            {
                centerHandle.Position = Corners.RectCenter;
                centerHandle.Draw(context);
            }
        }

        int saved = context.Save();
        if (ShowHandles && rotationCursorActive)
        {
            var matrix = Matrix3X3.CreateTranslation((float)lastPointerPos.X, (float)lastPointerPos.Y);
            double angle = (lastPointerPos - InternalState.Origin).Angle * 180 / Math.PI - 90;
            matrix = matrix.PostConcat(Matrix3X3.CreateRotationDegrees((float)angle, (float)lastPointerPos.X,
                (float)lastPointerPos.Y));
            matrix = matrix.PostConcat(Matrix3X3.CreateScale(7f / (float)ZoomScale, 7f / (float)ZoomScale,
                (float)lastPointerPos.X, (float)lastPointerPos.Y));
            context.SetMatrix(context.TotalMatrix.Concat(matrix));

            context.DrawPath(rotateCursorGeometry, whiteFillPen);
            context.DrawPath(rotateCursorGeometry, cursorBorderPaint);
        }

        context.RestoreToCount(saved);

        saved = context.Save();

        if (ShowHandles && shearCursorActive)
        {
            var matrix = Matrix3X3.CreateTranslation((float)lastPointerPos.X, (float)lastPointerPos.Y);

            matrix = matrix.PostConcat(Matrix3X3.CreateTranslation(
                (float)-shearCursorGeometry.VisualAABB.Center.X,
                (float)-shearCursorGeometry.VisualAABB.Center.Y));

            matrix = matrix.PostConcat(Matrix3X3.CreateScale(
                20 / zoomboxScale / (float)shearCursorGeometry.VisualAABB.Size.X,
                20 / zoomboxScale / (float)shearCursorGeometry.VisualAABB.Size.Y,
                (float)lastPointerPos.X, (float)lastPointerPos.Y));

            if (hoveredAnchor is Anchor.Right or Anchor.Left)
                matrix = matrix.PostConcat(Matrix3X3.CreateRotationDegrees(90, (float)lastPointerPos.X,
                    (float)lastPointerPos.Y));

            context.SetMatrix(context.TotalMatrix.Concat(matrix));

            shearCursorGeometry.RasterizeTransformed(context);
        }

        context.RestoreToCount(saved);

        if (IsSizeBoxEnabled)
        {
            int toRestore = context.Save();
            var matrix = context.TotalMatrix;
            VecD pos = matrix.MapPoint(lastPointerPos);
            context.SetMatrix(Matrix3X3.Identity);

            if (isRotating)
            {
                infoBox.DrawInfo(context, $"{(RadiansToDegreesNormalized(corners.RectRotation)):0.#}\u00b0",
                    pos);
            }
            else
            {
                VecD rectSize = Corners.RectSize;
                string sizeText = $"W: {rectSize.X:0.#} H: {rectSize.Y:0.#} px";
                infoBox.DrawInfo(context, sizeText, pos);
            }

            context.RestoreToCount(toRestore);
        }
    }

    private double RadiansToDegreesNormalized(double radians)
    {
        double degrees = double.RadiansToDegrees(radians);
        degrees = (degrees + 360) % 360;
        return degrees;
    }

    private void OnAnchorHandlePressed(Handle source, OverlayPointerArgs args)
    {
        CaptureAnchor(anchorMap[source]);

        if (source == originHandle)
        {
            IsSizeBoxEnabled = false;
            snapHandleOfOrigin = null;
        }

        IsSizeBoxEnabled = true;
    }

    private void CaptureAnchor(Anchor anchor)
    {
        capturedAnchor = anchor;
        cornersOnStartAnchorDrag = Corners;
        originOnStartAnchorDrag = InternalState.Origin;
        mousePosOnStartAnchorDrag = lastPointerPos;
    }

    private void OnMoveHandlePressed(Handle source, OverlayPointerArgs args)
    {
        StartMoving(args.Point);
    }

    protected override void OnOverlayPointerExited(OverlayPointerArgs args)
    {
        rotationCursorActive = false;
        shearCursorActive = false;
        Refresh();
    }

    protected override void OnOverlayPointerPressed(OverlayPointerArgs args)
    {
        if (args.PointerButton != MouseButton.Left)
            return;

        lastClickCount = args.ClickCount;

        if (Handles.Any(x => x.IsWithinHandle(x.Position, args.Point, ZoomScale))) return;

        pressedWithinBounds = TestHit(args.Point);

        if (CanShear(args.Point, out var side))
        {
            StartShearing(args, side);
        }
        else if (!CanRotate(args.Point))
        {
            StartMoving(args.Point);
        }
        else if (!LockRotation)
        {
            isRotating = true;
            mousePosOnStartRotate = args.Point;
            cornersOnStartRotate = Corners;
            propAngle1OnStartRotate = InternalState.ProportionalAngle1;
            propAngle2OnStartRotate = InternalState.ProportionalAngle2;
        }
        else
        {
            return;
        }

        IsSizeBoxEnabled = true;
        args.Pointer.Capture(this);
        args.Handled = true;
    }

    private void OnAnchorHandleDrag(Handle source, OverlayPointerArgs args)
    {
        HandleCapturedAnchorMovement(args.Point);
        lastPointerPos = args.Point;
    }

    private void OnMoveHandleDrag(Handle source, OverlayPointerArgs args)
    {
        HandleTransform(lastPointerPos);
        Cursor = new Cursor(StandardCursorType.DragMove);
        actuallyMoved = true;
        lastPointerPos = args.Point;
    }

    protected override void OnOverlayPointerMoved(OverlayPointerArgs e)
    {
        Cursor finalCursor = new Cursor(StandardCursorType.Arrow);

        lastPointerPos = e.Point;
        VecD pos = lastPointerPos;

        if (isMoving)
        {
            HandleTransform(pos);
            finalCursor = new Cursor(StandardCursorType.DragMove);
            actuallyMoved = true;
        }

        if (capturedAnchor is not null)
        {
            HandleCapturedAnchorMovement(e.Point);
            lastPointerPos = e.Point;
            return;
        }

        if (UpdateSpecialCursors(e.Point))
        {
            finalCursor = new Cursor(StandardCursorType.None);
        }

        Anchor? anchor =
            TransformHelper.GetAnchorInPosition(pos, Corners, InternalState.Origin, ZoomScale, topLeftHandle.Size);

        if (isRotating)
        {
            finalCursor = HandleRotate(pos);
        }
        else if (anchor is not null)
        {
            if ((TransformHelper.IsCorner((Anchor)anchor) && CornerFreedom == TransformCornerFreedom.Free) ||
                (TransformHelper.IsSide((Anchor)anchor) && SideFreedom == TransformSideFreedom.Free))
                finalCursor = new Cursor(StandardCursorType.Arrow);
            else
                finalCursor = TransformHelper.GetResizeCursor((Anchor)anchor, Corners, ZoomboxAngle);
        }

        if (!ShowHandles)
        {
            finalCursor = new Cursor(StandardCursorType.Arrow);
        }

        if (Cursor != finalCursor)
            Cursor = finalCursor;

        Refresh();
    }

    protected override void OnOverlayPointerReleased(OverlayPointerArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Left)
        {
            pressedWithinBounds = false;
            return;
        }

        if (!isRotating && !actuallyMoved && pressedWithinBounds)
        {
            MouseOnCanvasEventArgs args = new(MouseButton.Left, e.Point, e.Modifiers, lastClickCount);
            PassthroughPointerPressedCommand?.Execute(args);
            lastClickCount = 0;
        }

        if (isRotating)
        {
            isRotating = false;
            e.Pointer.Capture(null);
            Cursor = new Cursor(StandardCursorType.Arrow);
            var pos = e.Point;
            UpdateSpecialCursors(pos);
        }

        if (isShearing)
        {
            isShearing = false;
            SideFreedom = beforeShearSideFreedom;
            e.Pointer.Capture(null);
            Cursor = new Cursor(StandardCursorType.Arrow);
            var pos = e.Point;
            UpdateSpecialCursors(pos);
        }

        StopMoving();
        IsSizeBoxEnabled = false;
        capturedAnchor = null;
        pressedWithinBounds = false;
    }

    public override bool TestHit(VecD point)
    {
        const double offsetInPixels = 30;
        double offsetToScale = offsetInPixels / ZoomScale;
        ShapeCorners scaled = Corners.AsRotated(-Corners.RectRotation, Corners.RectCenter);
        ShapeCorners scaledCorners = new ShapeCorners()
        {
            BottomLeft = scaled.BottomLeft - new VecD(offsetToScale, -offsetToScale),
            BottomRight = scaled.BottomRight + new VecD(offsetToScale, offsetToScale),
            TopLeft = scaled.TopLeft - new VecD(offsetToScale, offsetToScale),
            TopRight = scaled.TopRight - new VecD(-offsetToScale, offsetToScale),
        };

        scaledCorners = scaledCorners.AsRotated(Corners.RectRotation, Corners.RectCenter);

        return base.TestHit(point) || scaledCorners.IsPointInside(point);
    }

    private void OnMoveHandleReleased(Handle obj, OverlayPointerArgs args)
    {
        StopMoving();
    }

    private bool CanShear(VecD mousePos, out Anchor side)
    {
        if (LockShear)
        {
            side = default;
            return false;
        }

        double distance = 20 / ZoomScale;
        var sides = new[] { Anchor.Top, Anchor.Bottom, Anchor.Left, Anchor.Right };

        bool isOverHandle = Handles.Any(x => x.IsWithinHandle(x.Position, mousePos, ZoomScale));
        if (isOverHandle)
        {
            side = default;
            return false;
        }

        side = sides.FirstOrDefault(side => VecD.Distance(TransformHelper.GetAnchorPosition(Corners, side), mousePos)
                                            < distance);

        return side != default && !Corners.IsPointInside(mousePos);
    }

    private void StopMoving()
    {
        isMoving = false;

        if (ActionCompleted is not null && ActionCompleted.CanExecute(null))
            ActionCompleted.Execute(null);

        HighlightSnappedAxis(null, null);
        IsSizeBoxEnabled = false;
    }

    private void StartMoving(VecD position)
    {
        isMoving = true;
        mousePosOnStartMove = position;
        originOnStartMove = InternalState.Origin;
        cornersOnStartMove = Corners;
        actuallyMoved = false;
    }

    private void StartShearing(OverlayPointerArgs args, Anchor side)
    {
        isShearing = true;
        beforeShearSideFreedom = SideFreedom;
        SideFreedom = TransformSideFreedom.Shear;
        CaptureAnchor(side);
        lastPointerPos = args.Point;
    }

    private void HandleTransform(VecD pos)
    {
        VecD delta = pos - mousePosOnStartMove;

        if (Corners.IsAlignedToPixels && CanAlignToPixels)
            delta = delta.Round();

        ShapeCorners rawCorners = new ShapeCorners()
        {
            BottomLeft = cornersOnStartMove.BottomLeft + delta,
            BottomRight = cornersOnStartMove.BottomRight + delta,
            TopLeft = cornersOnStartMove.TopLeft + delta,
            TopRight = cornersOnStartMove.TopRight + delta,
        };

        var snapDeltaResult = TrySnapCorners(rawCorners);

        VecD snapDelta = snapDeltaResult.Delta;

        HighlightSnappedAxis(snapDeltaResult.SnapAxisXName, snapDeltaResult.SnapAxisYName, snapDeltaResult.SnapSource);

        VecD from = originOnStartMove;

        Corners = ApplyCornersWithDelta(cornersOnStartMove, delta, snapDelta);

        InternalState = InternalState with { Origin = originOnStartMove + delta + snapDelta };

        VecD to = InternalState.Origin;
        TransformDraggedCommand?.Execute((from, to));
    }

    private ShapeCorners ApplyCornersWithDelta(ShapeCorners corners, VecD delta, VecD snapDelta)
    {
        return new ShapeCorners()
        {
            BottomLeft = corners.BottomLeft + delta + snapDelta,
            BottomRight = corners.BottomRight + delta + snapDelta,
            TopLeft = corners.TopLeft + delta + snapDelta,
            TopRight = corners.TopRight + delta + snapDelta,
        };
    }

    private SnapData TrySnapCorners(ShapeCorners rawCorners)
    {
        if (SnappingController is null)
        {
            return new SnapData();
        }

        VecD[] pointsToTest = new VecD[]
        {
            rawCorners.RectCenter, rawCorners.TopLeft, rawCorners.TopRight, rawCorners.BottomLeft,
            rawCorners.BottomRight, rawCorners.TopCenter, rawCorners.BottomCenter, rawCorners.LeftCenter,
            rawCorners.RightCenter
        };

        VecD snapDelta = SnappingController.GetSnapDeltaForPoints(pointsToTest, out string snapAxisX,
            out string snapAxisY, out VecD? snapSource);

        return new SnapData()
        {
            Delta = snapDelta,
            SnapSource = snapSource + snapDelta,
            SnapAxisXName = snapAxisX,
            SnapAxisYName = snapAxisY
        };
    }

    private Cursor HandleRotate(VecD pos)
    {
        Cursor finalCursor;
        finalCursor = new Cursor(StandardCursorType.None);
        double angle = (mousePosOnStartRotate - InternalState.Origin).CCWAngleTo(pos - InternalState.Origin);
        if (SnapToAngles)
            angle = TransformHelper.FindSnappingAngle(cornersOnStartRotate, angle);
        InternalState = InternalState with
        {
            ProportionalAngle1 = propAngle1OnStartRotate + angle,
            ProportionalAngle2 = propAngle2OnStartRotate + angle,
        };

        Corners = TransformUpdateHelper.UpdateShapeFromRotation(cornersOnStartRotate, InternalState.Origin, angle);

        return finalCursor;
    }

    private bool CanRotate(VecD mousePos)
    {
        return !Corners.IsPointInside(mousePos) &&
               Handles.All(x => !x.IsWithinHandle(x.Position, mousePos, ZoomScale)) && TestHit(mousePos);
    }

    private bool UpdateSpecialCursors(VecD mousePos)
    {
        bool canShear = CanShear(mousePos, out Anchor anchor);
        if ((!canShear && !CanRotate(mousePos) && !isRotating) || LockRotation)
        {
            rotationCursorActive = false;
            shearCursorActive = false;
            return false;
        }

        rotationCursorActive = !canShear;
        shearCursorActive = canShear;
        hoveredAnchor = anchor;
        return true;
    }

    private void HandleCapturedAnchorMovement(VecD point)
    {
        if (capturedAnchor is null)
            throw new InvalidOperationException("No anchor is captured");

        if ((TransformHelper.IsCorner((Anchor)capturedAnchor) && CornerFreedom == TransformCornerFreedom.Locked) ||
            (TransformHelper.IsSide((Anchor)capturedAnchor) && SideFreedom == TransformSideFreedom.Locked))
            return;

        pos = point;

        if (TransformHelper.IsCorner((Anchor)capturedAnchor))
        {
            VecD targetPos = TransformHelper.GetAnchorPosition(cornersOnStartAnchorDrag, (Anchor)capturedAnchor) + pos -
                             mousePosOnStartAnchorDrag;

            ShapeCorners? newCorners = TransformUpdateHelper.UpdateShapeFromCorner
            ((Anchor)capturedAnchor, CornerFreedom, InternalState.ProportionalAngle1,
                InternalState.ProportionalAngle2, cornersOnStartAnchorDrag, targetPos,
                ScaleFromCenter,
                SnappingController,
                out string snapX, out string snapY, out VecD? snapPoint);

            HighlightSnappedAxis(snapX, snapY, snapPoint);

            if (newCorners is not null)
            {
                bool shouldAlign =
                    (CornerFreedom is TransformCornerFreedom.ScaleProportionally or TransformCornerFreedom.Scale) &&
                    Corners.IsAlignedToPixels;

                newCorners = shouldAlign
                    ? TransformHelper.AlignToPixels((ShapeCorners)newCorners)
                    : (ShapeCorners)newCorners;

                Corners = (ShapeCorners)newCorners;
            }

            UpdateOriginPos();
        }
        else if (TransformHelper.IsSide((Anchor)capturedAnchor))
        {
            // Mouse position is projected onto the line from the rect origin to the anchor being dragged,
            // otherwise mouse could be somewhere else and delta wouldn't be a straight line.
            VecD originalAnchorPos =
                TransformHelper.GetAnchorPosition(cornersOnStartAnchorDrag, (Anchor)capturedAnchor);
            VecD targetPos = originalAnchorPos + pos - mousePosOnStartAnchorDrag;

            VecD projected = targetPos.ProjectOntoLine(originalAnchorPos, InternalState.Origin);
            if (projected.IsNaNOrInfinity())
            {
                projected = targetPos;
            }

            VecD anchorRelativeDelta = projected - originalAnchorPos;

            var adjacentAnchors = TransformHelper.GetAdjacentAnchors((Anchor)capturedAnchor);
            SnapData snapped = new SnapData();

            if (SideFreedom is TransformSideFreedom.Shear or TransformSideFreedom.Free)
            {
                VecD rawDelta = targetPos - originalAnchorPos;
                VecD adjacentPos = TransformHelper.GetAnchorPosition(cornersOnStartAnchorDrag, adjacentAnchors.Item1);
                snapped = TrySnapAnchor(adjacentPos + rawDelta);

                if (snapped.Delta == VecD.Zero)
                {
                    snapped = TrySnapAnchor(targetPos);
                }

                if (snapped.Delta == VecD.Zero)
                {
                    adjacentPos = TransformHelper.GetAnchorPosition(cornersOnStartAnchorDrag, adjacentAnchors.Item2);
                    snapped = TrySnapAnchor(adjacentPos + rawDelta);
                }
            }
            else if (SideFreedom is not TransformSideFreedom.ScaleProportionally)
            {
                // If rotation is almost cardinal, projecting snapping points result in extreme values when perpendicular to the axis
                if (!TransformHelper.RotationIsAlmostCardinal(cornersOnStartAnchorDrag.RectRotation))
                {
                    snapped = FindProjectedAnchorSnap(projected);
                    if (snapped.Delta == VecI.Zero)
                    {
                        snapped = FindAdjacentCornersSnap(adjacentAnchors, anchorRelativeDelta);
                    }
                }
                else
                {
                    snapped = TrySnapAnchor(targetPos);
                }
            }

            VecD potentialPos = targetPos + snapped.Delta;
            if (potentialPos.X < 0 || potentialPos.Y < 0 || potentialPos.X > lastSize.X || potentialPos.Y > lastSize.Y)
            {
                snapped = new SnapData();
            }

            ShapeCorners? newCorners = TransformUpdateHelper.UpdateShapeFromSide
            ((Anchor)capturedAnchor, SideFreedom, InternalState.ProportionalAngle1,
                InternalState.ProportionalAngle2, cornersOnStartAnchorDrag, targetPos + snapped.Delta,
                ScaleFromCenter,
                SnappingController, out string snapX, out string snapY);

            string finalSnapX = snapped.SnapAxisXName ?? snapX;
            string finalSnapY = snapped.SnapAxisYName ?? snapY;
            VecD? finalSnapPoint = null;
            if (newCorners.HasValue && snapped.Delta != VecD.Zero)
            {
                finalSnapPoint = TransformHelper.GetAnchorPosition(newCorners.Value, (Anchor)capturedAnchor);
            }

            HighlightSnappedAxis(finalSnapX, finalSnapY, finalSnapPoint);

            if (newCorners is not null)
            {
                bool shouldAlign =
                    (SideFreedom is TransformSideFreedom.ScaleProportionally or TransformSideFreedom.Stretch) &&
                    Corners.IsAlignedToPixels;
                Corners = shouldAlign
                    ? TransformHelper.AlignToPixels((ShapeCorners)newCorners)
                    : (ShapeCorners)newCorners;
            }

            UpdateOriginPos();
        }
        else if (capturedAnchor == Anchor.Origin)
        {
            pos = HandleSnap(pos, out bool snapped);
            InternalState = InternalState with { OriginWasManuallyDragged = !snapped, Origin = pos, };
        }

        Refresh();
    }

    private ShapeCorners SnapAnchorInCorners(Anchor anchor, ShapeCorners corners, VecD delta)
    {
        var newCorners = SnapSelectedAnchorsCorners(corners, delta, anchor);

        return newCorners;
    }

    private static ShapeCorners SnapSelectedAnchorsCorners(ShapeCorners corners, VecD delta, Anchor anchor)
    {
        VecD anchorPos = TransformHelper.GetAnchorPosition(corners, anchor);
        VecD targetAnchorPos = anchorPos + delta;

        VecD topLeftPos = corners.TopLeft;
        VecD topRightPos = corners.TopRight;
        VecD bottomLeftPos = corners.BottomLeft;
        VecD bottomRightPos = corners.BottomRight;

        if (anchor == Anchor.TopLeft)
        {
            topLeftPos = targetAnchorPos;
            topRightPos = new VecD(topRightPos.X, topRightPos.Y + delta.Y);
            bottomLeftPos = new VecD(bottomLeftPos.X + delta.X, bottomLeftPos.Y);
        }
        else if (anchor == Anchor.TopRight)
        {
            topRightPos = targetAnchorPos;
            topLeftPos = new VecD(topLeftPos.X, topLeftPos.Y + delta.Y);
            bottomRightPos = new VecD(bottomRightPos.X + delta.X, bottomRightPos.Y);
        }
        else if (anchor == Anchor.BottomLeft)
        {
            bottomLeftPos = targetAnchorPos;
            topLeftPos = new VecD(topLeftPos.X + delta.X, topLeftPos.Y);
            bottomRightPos = new VecD(bottomRightPos.X, bottomRightPos.Y + delta.Y);
        }
        else if (anchor == Anchor.BottomRight)
        {
            bottomRightPos = targetAnchorPos;
            topRightPos = new VecD(topRightPos.X + delta.X, topRightPos.Y);
            bottomLeftPos = new VecD(bottomLeftPos.X, bottomLeftPos.Y + delta.Y);
        }

        return new ShapeCorners()
        {
            TopLeft = topLeftPos, TopRight = topRightPos, BottomLeft = bottomLeftPos, BottomRight = bottomRightPos,
        };
    }

    private SnapData FindAdjacentCornersSnap((Anchor, Anchor) adjacentAnchors, VecD anchorRelativeDelta)
    {
        VecD adjacentAnchorPos =
            TransformHelper.GetAnchorPosition(cornersOnStartAnchorDrag, adjacentAnchors.Item1) +
            anchorRelativeDelta;

        var originAdj = TransformHelper.GetAdjacentAnchors(adjacentAnchors.Item1);
        var adjacent = originAdj.Item1 == capturedAnchor ? originAdj.Item2 : originAdj.Item1;

        VecD snapOrigin = TransformHelper.GetAnchorPosition(cornersOnStartAnchorDrag, adjacent) + anchorRelativeDelta;
        var snapped = TrySnapAnchorAlongLine(adjacentAnchorPos, snapOrigin);
        double maxDistance = GetSizeToOppositeSide(cornersOnStartAnchorDrag, capturedAnchor.Value) / 8f;

        if (snapped.Delta.Length > maxDistance)
        {
            snapped = new SnapData();
        }

        if (snapped.Delta == VecI.Zero)
        {
            adjacentAnchorPos = TransformHelper.GetAnchorPosition(cornersOnStartAnchorDrag, adjacentAnchors.Item2) +
                                anchorRelativeDelta;
            originAdj = TransformHelper.GetAdjacentAnchors(adjacentAnchors.Item2);
            adjacent = originAdj.Item1 == capturedAnchor ? originAdj.Item2 : originAdj.Item1;
            snapOrigin = TransformHelper.GetAnchorPosition(cornersOnStartAnchorDrag, adjacent) + anchorRelativeDelta;

            snapped = TrySnapAnchorAlongLine(adjacentAnchorPos, snapOrigin);
            if (snapped.Delta.Length > maxDistance)
            {
                snapped = new SnapData();
            }
        }

        return snapped;
    }

    private double GetSizeToOppositeSide(ShapeCorners corners, Anchor anchor1)
    {
        Anchor opposite = TransformHelper.GetOppositeAnchor(anchor1);
        VecD oppositePos = TransformHelper.GetAnchorPosition(corners, opposite);
        VecD anchorPos = TransformHelper.GetAnchorPosition(corners, anchor1);
        return (oppositePos - anchorPos).Length;
    }

    private SnapData FindProjectedAnchorSnap(VecD projected)
    {
        VecD origin = InternalState.Origin;
        var snapped = TrySnapAnchorAlongLine(projected, origin);

        return snapped;
    }

    // https://www.desmos.com/calculator/drdxuriovg
    private SnapData TrySnapAnchorAlongLine(VecD anchor, VecD origin)
    {
        if (SnappingController is null)
        {
            return new SnapData();
        }

        VecD[] pointsToTest = new VecD[] { anchor };

        VecD snapDelta = SnappingController.GetSnapDeltaForPoints(pointsToTest, out string snapAxisX,
            out string snapAxisY, out VecD? snapSource);

        // snap delta is a straight line from the anchor to the snapped point, we need to find intersection between snap point axis and line starting from projectLineStart going through transformed
        VecD snapPoint = anchor + snapDelta;

        VecD horizontalIntersection = FindHorizontalIntersection(origin, anchor, snapPoint.Y);
        VecD verticalIntersection = FindVerticalIntersection(origin, anchor, snapPoint.X);

        snapPoint = string.IsNullOrEmpty(snapAxisX) ? horizontalIntersection : verticalIntersection;

        snapDelta = snapPoint - anchor;

        if (string.IsNullOrEmpty(snapAxisY) && string.IsNullOrEmpty(snapAxisX))
        {
            snapDelta = VecD.Zero;
        }

        return new SnapData()
        {
            Delta = snapDelta, SnapSource = snapSource, SnapAxisXName = snapAxisX, SnapAxisYName = snapAxisY
        };
    }

    private VecD FindHorizontalIntersection(VecD p1, VecD p2, double y)
    {
        double slope = (p2.Y - p1.Y) / (p2.X - p1.X);
        if (slope == 0 || double.IsInfinity(slope))
        {
            return new VecD(p2.X, y);
        }

        double yIntercept = p1.Y - slope * p1.X;
        double x = (y - yIntercept) / slope;

        return new VecD(x, y);
    }

    private VecD FindVerticalIntersection(VecD p1, VecD p2, double x)
    {
        double slope = (p2.Y - p1.Y) / (p2.X - p1.X);
        if (slope == 0 || double.IsInfinity(slope))
        {
            return new VecD(x, p2.Y);
        }

        double yIntercept = p1.Y - slope * p1.X;
        double y = slope * x + yIntercept;

        return new VecD(x, y);
    }

    private SnapData TrySnapAnchor(VecD anchorPos)
    {
        if (SnappingController is null)
        {
            return new SnapData();
        }

        VecD[] pointsToTest = new VecD[] { anchorPos };

        VecD snapDelta = SnappingController.GetSnapDeltaForPoints(pointsToTest, out string snapAxisX,
            out string snapAxisY, out VecD? snapSource);

        return new SnapData()
        {
            Delta = snapDelta, SnapSource = snapSource, SnapAxisXName = snapAxisX, SnapAxisYName = snapAxisY
        };
    }

    private void HighlightSnappedAxis(string snapAxisXName, string snapAxisYName, VecD? snapSource = null)
    {
        SnappingController.HighlightedXAxis = snapAxisXName;
        SnappingController.HighlightedYAxis = snapAxisYName;
        SnappingController.HighlightedPoint = snapSource;
    }

    private void UpdateOriginPos()
    {
        if (!InternalState.OriginWasManuallyDragged)
        {
            if (snapHandleOfOrigin == centerHandle)
            {
                snapHandleOfOrigin.Position = TransformHelper.OriginFromCorners(Corners);
            }

            InternalState = InternalState with
            {
                Origin = snapHandleOfOrigin?.Position ?? TransformHelper.OriginFromCorners(Corners)
            };
        }
    }

    private VecD HandleSnap(VecD pos, out bool snapped)
    {
        foreach (var snapPoint in snapPoints)
        {
            if (snapPoint == originHandle)
                continue;

            if (TransformHelper.IsWithinHandle(snapPoint.Position, pos, ZoomScale, topHandle.Size))
            {
                snapped = true;
                return snapPoint.Position;
            }
        }

        snapped = false;
        return originOnStartAnchorDrag + pos - mousePosOnStartAnchorDrag;
    }

    private void OnAnchorHandleReleased(Handle source, OverlayPointerArgs args)
    {
        capturedAnchor = null;

        if (source == originHandle)
        {
            snapHandleOfOrigin = GetSnapHandleOfOrigin();
            InternalState = InternalState with { OriginWasManuallyDragged = snapHandleOfOrigin is null };
        }

        if (ActionCompleted is not null && ActionCompleted.CanExecute(null))
            ActionCompleted.Execute(null);

        IsSizeBoxEnabled = false;

        HighlightSnappedAxis(null, null);
    }

    private Handle? GetSnapHandleOfOrigin()
    {
        foreach (var snapPoint in snapPoints)
        {
            if (snapPoint == originHandle)
                continue;

            if (originHandle.Position == snapPoint.Position)
            {
                return snapPoint;
            }
        }

        return null;
    }

    private void OnRequestedCorners(object sender, ShapeCorners corners)
    {
        isMoving = false;
        isRotating = false;
        if (isShearing)
        {
            SideFreedom = beforeShearSideFreedom;
        }

        isShearing = false;
        Corners = corners;
        InternalState = new()
        {
            ProportionalAngle1 = (Corners.BottomRight - Corners.TopLeft).Angle,
            ProportionalAngle2 = (Corners.TopRight - Corners.BottomLeft).Angle,
            OriginWasManuallyDragged = false,
            Origin = TransformHelper.OriginFromCorners(Corners),
        };
    }

    private static void OnCornersExecutorChanged(AvaloniaPropertyChangedEventArgs<ExecutionTrigger<ShapeCorners>> args)
    {
        TransformOverlay overlay = (TransformOverlay)args.Sender;
        if (args.OldValue != null)
            args.OldValue.Value.Triggered -= overlay.OnRequestedCorners;
        if (args.NewValue != null)
            args.NewValue.Value.Triggered += overlay.OnRequestedCorners;
    }
}

struct SnapData
{
    public VecD Delta { get; set; }
    public string SnapAxisXName { get; set; }
    public string SnapAxisYName { get; set; }
    public VecD? SnapSource { get; set; }
}
