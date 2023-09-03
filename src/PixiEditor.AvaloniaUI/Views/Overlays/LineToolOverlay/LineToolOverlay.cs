using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Controllers.InputDevice;
using PixiEditor.AvaloniaUI.Views.Overlays.TransformOverlay;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Views.UserControls.Overlays.LineToolOverlay;

namespace PixiEditor.AvaloniaUI.Views.Overlays.LineToolOverlay;
internal class LineToolOverlay : Overlay
{
    public static readonly StyledProperty<double> ZoomboxScaleProperty =
        AvaloniaProperty.Register<LineToolOverlay, double>(nameof(ZoomboxScale), defaultValue: 1.0);

    public double ZoomboxScale
    {
        get => GetValue(ZoomboxScaleProperty);
        set => SetValue(ZoomboxScaleProperty, value);
    }

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

    static LineToolOverlay()
    {
        AffectsRender<LineToolOverlay>(ZoomboxScaleProperty, LineStartProperty, LineEndProperty);

        ZoomboxScaleProperty.Changed.Subscribe(OnZoomboxScaleChanged);
    }

    private Pen blackPen = new Pen(Brushes.Black, 1);

    private VecD mouseDownPos = VecD.Zero;
    private VecD lineStartOnMouseDown = VecD.Zero;
    private VecD lineEndOnMouseDown = VecD.Zero;

    private LineToolOverlayAnchor? capturedAnchor = null;
    private bool dragging = false;
    private bool movedWhileMouseDown = false;

    private Geometry handleGeometry = GetHandleGeometry("MoveHandle");

    private MouseUpdateController mouseUpdateController;

    public LineToolOverlay()
    {
        Cursor = new Cursor(StandardCursorType.Arrow);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        mouseUpdateController = new MouseUpdateController(this, MouseMoved);
    }

    private static void OnZoomboxScaleChanged(AvaloniaPropertyChangedEventArgs<double> args)
    {
        var self = (LineToolOverlay)args.Sender;
        double newScale = args.NewValue.Value;
        self.blackPen.Thickness = 1.0 / newScale;
    }

    public override void Render(DrawingContext context)
    {
        float scaleMultiplier = (float)(1.0 / ZoomboxScale);
        float radius = 2.5f * scaleMultiplier;

        context.DrawRectangle(BackgroundBrush, blackPen, TransformHelper.ToAnchorRect(LineStart, ZoomboxScale), radius, radius);
        context.DrawRectangle(BackgroundBrush, blackPen, TransformHelper.ToAnchorRect(LineEnd, ZoomboxScale), radius, radius);

        VecD handlePos = TransformHelper.GetDragHandlePos(new ShapeCorners(new RectD(LineStart, LineEnd - LineStart)), ZoomboxScale);
        const double CrossSize = TransformHelper.MoveHandleSize - 1;
        context.DrawRectangle(BackgroundBrush, blackPen, TransformHelper.ToHandleRect(handlePos, ZoomboxScale), radius, radius);
        handleGeometry.Transform = new MatrixTransform(
            new Matrix(
            0, CrossSize / ZoomboxScale,
            CrossSize / ZoomboxScale, 0,
            handlePos.X - CrossSize / (ZoomboxScale * 2), handlePos.Y - CrossSize / (ZoomboxScale * 2))
        );
        context.DrawGeometry(HandleGlyphBrush, null, handleGeometry);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        MouseButton changedButton = e.GetMouseButton(this);

        if (changedButton != MouseButton.Left)
            return;

        e.Handled = true;

        VecD pos = TransformHelper.ToVecD(e.GetPosition(this));
        VecD handlePos = TransformHelper.GetDragHandlePos(new ShapeCorners(new RectD(LineStart, LineEnd - LineStart)), ZoomboxScale);

        if (TransformHelper.IsWithinAnchor(LineStart, pos, ZoomboxScale))
            capturedAnchor = LineToolOverlayAnchor.Start;
        else if (TransformHelper.IsWithinAnchor(LineEnd, pos, ZoomboxScale))
            capturedAnchor = LineToolOverlayAnchor.End;
        else if (TransformHelper.IsWithinTransformHandle(handlePos, pos, ZoomboxScale))
            dragging = true;
        movedWhileMouseDown = false;

        mouseDownPos = pos;
        lineStartOnMouseDown = LineStart;
        lineEndOnMouseDown = LineEnd;

        e.Pointer.Capture(this);
    }

    protected void MouseMoved(PointerEventArgs e)
    {
        VecD pos = TransformHelper.ToVecD(e.GetPosition(this));
        if (capturedAnchor == LineToolOverlayAnchor.Start)
        {
            LineStart = pos;
            movedWhileMouseDown = true;
            return;
        }

        if (capturedAnchor == LineToolOverlayAnchor.End)
        {
            LineEnd = pos;
            movedWhileMouseDown = true;
            return;
        }

        if (dragging)
        {
            var delta = pos - mouseDownPos;
            LineStart = lineStartOnMouseDown + delta;
            LineEnd = lineEndOnMouseDown + delta;
            movedWhileMouseDown = true;
            return;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left)
            return;

        e.Handled = true;
        capturedAnchor = null;
        dragging = false;
        if (movedWhileMouseDown && ActionCompleted is not null && ActionCompleted.CanExecute(null))
            ActionCompleted.Execute(null);

        e.Pointer.Capture(null);
    }
}
