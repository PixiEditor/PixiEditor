using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.Controllers.InputDevice;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.Extensions.UI.Overlays;
using Drawie.Numerics;
using PixiEditor.Views.Overlays.Drawables;
using PixiEditor.Views.Overlays.Handles;
using PixiEditor.Views.Overlays.TransformOverlay;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;
using Point = Avalonia.Point;

namespace PixiEditor.Views.Overlays.LineToolOverlay;

internal class LineToolOverlay : Overlay
{
    public static readonly StyledProperty<VecD> LineStartProperty =
        AvaloniaProperty.Register<LineToolOverlay, VecD>(nameof(LineStart), defaultValue: VecD.Zero);

    public VecD LineStart
    {
        get => GetValue(LineStartProperty);
        set => SetValue(LineStartProperty, value);
    }

    public static readonly StyledProperty<VecD> LineEndProperty =
        AvaloniaProperty.Register<LineToolOverlay, VecD>(nameof(LineEnd), defaultValue: VecD.Zero);

    public VecD LineEnd
    {
        get => GetValue(LineEndProperty);
        set => SetValue(LineEndProperty, value);
    }

    public static readonly StyledProperty<ICommand?> ActionCompletedProperty =
        AvaloniaProperty.Register<LineToolOverlay, ICommand?>(nameof(ActionCompleted));

    public ICommand? ActionCompleted
    {
        get => GetValue(ActionCompletedProperty);
        set => SetValue(ActionCompletedProperty, value);
    }

    public static readonly StyledProperty<SnappingController> SnappingControllerProperty =
        AvaloniaProperty.Register<LineToolOverlay, SnappingController>(
            nameof(SnappingController));

    public SnappingController SnappingController
    {
        get => GetValue(SnappingControllerProperty);
        set => SetValue(SnappingControllerProperty, value);
    }

    public static readonly StyledProperty<bool> ShowHandlesProperty = AvaloniaProperty.Register<LineToolOverlay, bool>(
        nameof(ShowHandles), defaultValue: true);

    public bool ShowHandles
    {
        get => GetValue(ShowHandlesProperty);
        set => SetValue(ShowHandlesProperty, value);
    }

    public static readonly StyledProperty<bool> IsSizeBoxEnabledProperty =
        AvaloniaProperty.Register<LineToolOverlay, bool>(
            nameof(IsSizeBoxEnabled));

    public bool IsSizeBoxEnabled
    {
        get => GetValue(IsSizeBoxEnabledProperty);
        set => SetValue(IsSizeBoxEnabledProperty, value);
    }

    public static readonly StyledProperty<ICommand> AddToUndoCommandProperty = AvaloniaProperty.Register<LineToolOverlay, ICommand>(
        nameof(AddToUndoCommand));

    public ICommand AddToUndoCommand
    {
        get => GetValue(AddToUndoCommandProperty);
        set => SetValue(AddToUndoCommandProperty, value);
    }

    static LineToolOverlay()
    {
        LineStartProperty.Changed.Subscribe(RenderAffectingPropertyChanged);
        LineEndProperty.Changed.Subscribe(RenderAffectingPropertyChanged);
    }


    private Paint blackPaint = new Paint()
    {
        Color = Colors.Black, StrokeWidth = 1, Style = PaintStyle.Stroke, IsAntiAliased = true
    };

    private VecD mouseDownPos = VecD.Zero;
    private VecD lineStartOnMouseDown = VecD.Zero;
    private VecD lineEndOnMouseDown = VecD.Zero;

    private VecD lastMousePos = VecD.Zero;

    private bool movedWhileMouseDown = false;

    private RectangleHandle startHandle;
    private RectangleHandle endHandle;
    private TransformHandle moveHandle;

    private bool isDraggingHandle = false;
    private InfoBox infoBox;

    private VecD startPos;
    private VecD endPos;

    public LineToolOverlay()
    {
        Cursor = new Cursor(StandardCursorType.Arrow);

        startHandle = new AnchorHandle(this);
        startHandle.StrokePaint = blackPaint;
        startHandle.OnPress += OnHandlePress;
        startHandle.OnDrag += StartHandleOnDrag;
        startHandle.OnHover += (handle, _) => Refresh();
        startHandle.OnRelease += OnHandleRelease;
        startHandle.Cursor = new Cursor(StandardCursorType.Arrow);
        AddHandle(startHandle);

        endHandle = new AnchorHandle(this);
        endHandle.StrokePaint = blackPaint;
        endHandle.OnPress += OnHandlePress;
        endHandle.OnDrag += EndHandleOnDrag;
        endHandle.Cursor = new Cursor(StandardCursorType.Arrow);
        endHandle.OnHover += (handle, _) => Refresh();
        endHandle.OnRelease += OnHandleRelease;
        AddHandle(endHandle);

        moveHandle = new TransformHandle(this);
        moveHandle.StrokePaint = blackPaint;
        moveHandle.OnPress += OnHandlePress;
        moveHandle.OnDrag += MoveHandleOnDrag;
        moveHandle.OnRelease += OnHandleRelease;
        endHandle.Cursor = new Cursor(StandardCursorType.Arrow);
        moveHandle.OnHover += (handle, _) => Refresh();
        moveHandle.OnRelease += OnHandleRelease;
        AddHandle(moveHandle);

        infoBox = new InfoBox();
    }

    protected override void OnOverlayPointerMoved(OverlayPointerArgs args)
    {
        base.OnOverlayPointerMoved(args);

        lastMousePos = args.Point;
    }

    public override bool TestHit(VecD point)
    {
        return IsVisible;
    }

    private void OnHandleRelease(Handle obj, OverlayPointerArgs args)
    {
        if (SnappingController != null)
        {
            SnappingController.HighlightedXAxis = null;
            SnappingController.HighlightedYAxis = null;
            SnappingController.HighlightedPoint = null;
            Refresh();
        }

        isDraggingHandle = false;
        IsSizeBoxEnabled = false;
        
        AddToUndoCommand.Execute((LineStart, LineEnd));
    }

    protected override void ZoomChanged(double newZoom)
    {
        blackPaint.StrokeWidth = 1 / (float)newZoom;
    }

    public override void RenderOverlay(Canvas context, RectD canvasBounds)
    {
        VecD mappedStart = LineStart;
        VecD mappedEnd = LineEnd;

        startHandle.Position = mappedStart;
        endHandle.Position = mappedEnd;

        VecD center = (mappedStart + mappedEnd) / 2;
        VecD size = mappedEnd - mappedStart;

        moveHandle.Position = TransformHelper.GetHandlePos(new ShapeCorners(center, size), ZoomScale, moveHandle.Size);

        if (ShowHandles)
        {
            startHandle.Draw(context);
            endHandle.Draw(context);
            moveHandle.Draw(context);
        }

        if (IsSizeBoxEnabled)
        {
            int toRestore = context.Save();
            var matrix = context.TotalMatrix;
            VecD pos = matrix.MapPoint(lastMousePos);
            context.SetMatrix(Matrix3X3.Identity);

            string length = $"L: {(mappedEnd - mappedStart).Length:0.#} px";
            infoBox.DrawInfo(context, length, pos);

            context.RestoreToCount(toRestore);
        }
    }

    protected override void OnOverlayPointerPressed(OverlayPointerArgs args)
    {
        if (args.PointerButton != MouseButton.Left)
            return;

        movedWhileMouseDown = false;
        mouseDownPos = args.Point;

        lineStartOnMouseDown = LineStart;
        lineEndOnMouseDown = LineEnd;

        args.Pointer.Capture(this);
    }

    private void StartHandleOnDrag(Handle source, OverlayPointerArgs args)
    {
        VecD delta = args.Point - mouseDownPos;
        LineStart = SnapAndHighlight(lineStartOnMouseDown + delta);
        movedWhileMouseDown = true;

        lastMousePos = args.Point;
        isDraggingHandle = true;
        IsSizeBoxEnabled = true;
    }

    private void EndHandleOnDrag(Handle source, OverlayPointerArgs args)
    {
        VecD delta = args.Point - mouseDownPos;
        VecD final = SnapAndHighlight(lineEndOnMouseDown + delta);

        LineEnd = final;
        movedWhileMouseDown = true;

        isDraggingHandle = true;
        lastMousePos = args.Point;
        IsSizeBoxEnabled = true;
    }

    private VecD SnapAndHighlight(VecD position)
    {
        VecD final = position;
        if (SnappingController != null)
        {
            double? x = SnappingController.SnapToHorizontal(position.X, out string snapAxisX);
            double? y = SnappingController.SnapToVertical(position.Y, out string snapAxisY);

            if (x.HasValue)
            {
                final = new VecD(x.Value, final.Y);
            }

            if (y.HasValue)
            {
                final = new VecD(final.X, y.Value);
            }

            SnappingController.HighlightedXAxis = snapAxisX;
            SnappingController.HighlightedYAxis = snapAxisY;
            SnappingController.HighlightedPoint = x != null || y != null ? final : null;
        }

        return final;
    }

    private void OnHandlePress(Handle source, OverlayPointerArgs args)
    {
        movedWhileMouseDown = false;
        mouseDownPos = args.Point;

        lineStartOnMouseDown = LineStart;
        lineEndOnMouseDown = LineEnd;
    }

    private void MoveHandleOnDrag(Handle source, OverlayPointerArgs args)
    {
        var delta = args.Point - mouseDownPos;

        VecD mappedStart = lineStartOnMouseDown;
        VecD mappedEnd = lineEndOnMouseDown;

        ((string, string), VecD) snapDeltaResult = TrySnapLine(mappedStart, mappedEnd, delta, out VecD? snapSource);

        if (SnappingController != null)
        {
            SnappingController.HighlightedXAxis = snapDeltaResult.Item1.Item1;
            SnappingController.HighlightedYAxis = snapDeltaResult.Item1.Item2;
            SnappingController.HighlightedPoint = snapSource;
        }

        LineStart = lineStartOnMouseDown + delta + snapDeltaResult.Item2;
        LineEnd = lineEndOnMouseDown + delta + snapDeltaResult.Item2;

        movedWhileMouseDown = true;
    }

    protected override void OnOverlayPointerReleased(OverlayPointerArgs args)
    {
        IsSizeBoxEnabled = false;

        if (args.InitialPressMouseButton != MouseButton.Left)
            return;

        if (movedWhileMouseDown && ActionCompleted is not null && ActionCompleted.CanExecute(null))
            ActionCompleted.Execute(null);
    }

    private ((string, string), VecD) TrySnapLine(VecD originalStart, VecD originalEnd, VecD delta, out VecD? snapSource)
    {
        if (SnappingController == null)
        {
            snapSource = null;
            return ((string.Empty, string.Empty), delta);
        }

        VecD center = (originalStart + originalEnd) / 2f;
        VecD[] pointsToTest = new VecD[] { center + delta, originalStart + delta, originalEnd + delta, };

        VecD snapDelta =
            SnappingController.GetSnapDeltaForPoints(pointsToTest, out string snapAxisX, out string snapAxisY,
                out snapSource);

        if (snapSource != null)
        {
            snapSource += snapDelta;
        }

        return ((snapAxisX, snapAxisY), snapDelta);
    }


    private static void RenderAffectingPropertyChanged(AvaloniaPropertyChangedEventArgs<VecD> e)
    {
        if (e.Sender is LineToolOverlay overlay)
        {
            overlay.Refresh();
        }
    }
}
