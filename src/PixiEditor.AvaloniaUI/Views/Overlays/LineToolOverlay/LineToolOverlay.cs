using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Controllers.InputDevice;
using PixiEditor.AvaloniaUI.Views.Overlays.Handles;
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

    private bool movedWhileMouseDown = false;

    private MouseUpdateController mouseUpdateController;

    private RectangleHandle startHandle;
    private RectangleHandle endHandle;
    private TransformHandle moveHandle;

    public LineToolOverlay()
    {
        Cursor = new Cursor(StandardCursorType.Arrow);

        VecD anchorSize = new(14, 14);

        startHandle = new RectangleHandle(this, LineStart, anchorSize);
        startHandle.HandlePen = blackPen;
        startHandle.OnDrag += StartHandleOnDrag;

        endHandle = new RectangleHandle(this, LineEnd, anchorSize);
        startHandle.HandlePen = blackPen;
        endHandle.OnDrag += EndHandleOnDrag;

        moveHandle = new TransformHandle(this, LineStart, new VecD(24, 24));
        moveHandle.HandlePen = blackPen;
        moveHandle.OnDrag += MoveHandleOnDrag;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        //TODO: Ensure this bug doesn't happen in Avalonia, currently Handle classes are taking care of dragging events
        //mouseUpdateController = new MouseUpdateController(this, MouseMoved);
    }

    private static void OnZoomboxScaleChanged(AvaloniaPropertyChangedEventArgs<double> args)
    {
        var self = (LineToolOverlay)args.Sender;
        double newScale = args.NewValue.Value;
        self.blackPen.Thickness = 1.0 / newScale;

        self.startHandle.ZoomboxScale = newScale;
        self.endHandle.ZoomboxScale = newScale;
        self.moveHandle.ZoomboxScale = newScale;
    }

    public override void Render(DrawingContext context)
    {
        startHandle.Position = LineStart;
        endHandle.Position = LineEnd;
        moveHandle.Position = TransformHelper.GetDragHandlePos(new ShapeCorners(LineStart, LineEnd - LineStart), ZoomboxScale);

        startHandle.Draw(context);
        endHandle.Draw(context);
        moveHandle.Draw(context);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        MouseButton changedButton = e.GetMouseButton(this);

        if (changedButton != MouseButton.Left)
            return;

        VecD pos = TransformHelper.ToVecD(e.GetPosition(this));

        movedWhileMouseDown = false;
        mouseDownPos = pos;
        lineStartOnMouseDown = LineStart;
        lineEndOnMouseDown = LineEnd;

        e.Pointer.Capture(this);
    }

    private void StartHandleOnDrag(VecD position)
    {
        LineStart = position;
        movedWhileMouseDown = true;
    }

    private void EndHandleOnDrag(VecD position)
    {
        LineEnd = position;
        movedWhileMouseDown = true;
    }

    private void MoveHandleOnDrag(VecD position)
    {
        var delta = position - mouseDownPos;

        LineStart = lineStartOnMouseDown + delta;
        LineEnd = lineEndOnMouseDown + delta;

        movedWhileMouseDown = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left)
            return;

        if (movedWhileMouseDown && ActionCompleted is not null && ActionCompleted.CanExecute(null))
            ActionCompleted.Execute(null);
    }
}
