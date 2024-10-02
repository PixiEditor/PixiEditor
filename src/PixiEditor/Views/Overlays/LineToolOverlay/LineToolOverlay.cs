using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.UI.Overlays;
using PixiEditor.Numerics;
using PixiEditor.Views.Overlays.Handles;
using PixiEditor.Views.Overlays.TransformOverlay;
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

    public static readonly StyledProperty<SnappingController> SnappingControllerProperty = AvaloniaProperty.Register<LineToolOverlay, SnappingController>(
        nameof(SnappingController));

    public SnappingController SnappingController
    {
        get => GetValue(SnappingControllerProperty);
        set => SetValue(SnappingControllerProperty, value);
    }

    static LineToolOverlay()
    {
        LineStartProperty.Changed.Subscribe(RenderAffectingPropertyChanged);
        LineEndProperty.Changed.Subscribe(RenderAffectingPropertyChanged);
    }

    private Pen blackPen = new Pen(Brushes.Black, 1);
    private Pen blackDashedPen = new Pen(Brushes.Black, 1) { DashStyle = new DashStyle(new double[] { 2, 4 }, 0) };
    private Pen whiteDashedPen = new Pen(Brushes.White, 1) { DashStyle = new DashStyle(new double[] { 2, 4 }, 2) };

    private VecD mouseDownPos = VecD.Zero;
    private VecD lineStartOnMouseDown = VecD.Zero;
    private VecD lineEndOnMouseDown = VecD.Zero;

    private bool movedWhileMouseDown = false;

    private RectangleHandle startHandle;
    private RectangleHandle endHandle;
    private TransformHandle moveHandle;

    private VecD startPos;
    private VecD endPos;

    public LineToolOverlay()
    {
        Cursor = new Cursor(StandardCursorType.Arrow);

        startHandle = new AnchorHandle(this);
        startHandle.HandlePen = blackPen;
        startHandle.OnDrag += StartHandleOnDrag;
        startHandle.OnHover += handle => Refresh();
        startHandle.OnRelease += OnHandleRelease;
        AddHandle(startHandle);

        endHandle = new AnchorHandle(this);
        endHandle.HandlePen = blackPen;
        endHandle.OnDrag += EndHandleOnDrag;
        endHandle.Cursor = new Cursor(StandardCursorType.Arrow);
        endHandle.OnHover += handle => Refresh();
        endHandle.OnRelease += OnHandleRelease;
        AddHandle(endHandle);

        moveHandle = new TransformHandle(this);
        moveHandle.HandlePen = blackPen;
        moveHandle.OnDrag += MoveHandleOnDrag;
        endHandle.Cursor = new Cursor(StandardCursorType.Arrow);
        moveHandle.OnHover += handle => Refresh();
        moveHandle.OnRelease += OnHandleRelease;
        AddHandle(moveHandle);
    }

    private void OnHandleRelease(Handle obj)
    {
        if (SnappingController != null)
        {
            SnappingController.HighlightedXAxis = null;
            SnappingController.HighlightedYAxis = null;
            Refresh();
        }
    }

    protected override void ZoomChanged(double newZoom)
    {
        blackPen.Thickness = 1.0 / newZoom;
        blackDashedPen.Thickness = 2.0 / newZoom;
        whiteDashedPen.Thickness = 2.0 / newZoom;
    }

    public override void RenderOverlay(DrawingContext context, RectD canvasBounds)
    {
        VecD mappedStart = LineStart;
        VecD mappedEnd = LineEnd;
        
        startHandle.Position = mappedStart;
        endHandle.Position = mappedEnd; 
        
        VecD center = (mappedStart + mappedEnd) / 2;
        VecD size = mappedEnd - mappedStart;
        
        moveHandle.Position = TransformHelper.GetHandlePos(new ShapeCorners(center, size), ZoomScale, moveHandle.Size);

        context.DrawLine(blackDashedPen, new Point(mappedStart.X, mappedStart.Y), new Point(mappedEnd.X, mappedEnd.Y));
        context.DrawLine(whiteDashedPen, new Point(mappedStart.X, mappedStart.Y), new Point(mappedEnd.X, mappedEnd.Y));
        
        startHandle.Draw(context);
        endHandle.Draw(context);
        moveHandle.Draw(context);
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

    private void StartHandleOnDrag(Handle source, VecD position)
    {
        VecD delta = position - mouseDownPos;
        LineStart = SnapAndHighlight(lineStartOnMouseDown + delta);
        movedWhileMouseDown = true;
    }

    private void EndHandleOnDrag(Handle source, VecD position)
    {
        VecD delta = position - mouseDownPos;
        VecD final = SnapAndHighlight(lineEndOnMouseDown + delta);
        
        LineEnd = final;
        movedWhileMouseDown = true;
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
        }

        return final;
    }

    private void MoveHandleOnDrag(Handle source, VecD position)
    {
        var delta = position - mouseDownPos;
        
        VecD mappedStart = lineStartOnMouseDown;
        VecD mappedEnd = lineEndOnMouseDown;

        ((string, string), VecD) snapDeltaResult = TrySnapLine(mappedStart, mappedEnd, delta);

        if (SnappingController != null)
        {
            SnappingController.HighlightedXAxis = snapDeltaResult.Item1.Item1;
            SnappingController.HighlightedYAxis = snapDeltaResult.Item1.Item2;
        }
        
        LineStart = lineStartOnMouseDown + delta + snapDeltaResult.Item2;
        LineEnd = lineEndOnMouseDown + delta + snapDeltaResult.Item2;

        movedWhileMouseDown = true;
    }

    protected override void OnOverlayPointerReleased(OverlayPointerArgs args)
    {
        if (args.InitialPressMouseButton != MouseButton.Left)
            return;

        if (movedWhileMouseDown && ActionCompleted is not null && ActionCompleted.CanExecute(null))
            ActionCompleted.Execute(null);
        
    }

    private ((string, string), VecD) TrySnapLine(VecD originalStart, VecD originalEnd, VecD delta)
    {
        if (SnappingController == null)
        {
            return ((string.Empty, string.Empty), delta);
        }
        
        VecD center = (originalStart + originalEnd) / 2f;
        VecD[] pointsToTest = new VecD[]
        {
            center + delta,
            originalStart + delta,
            originalEnd + delta,
        };

        VecD snapDelta = SnappingController.GetSnapDeltaForPoints(pointsToTest, out string snapAxisX, out string snapAxisY);

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
