using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Controllers;
using PixiEditor.Views.UserControls.Overlays.TransformOverlay;

namespace PixiEditor.Views.UserControls.Overlays.LineToolOverlay;
internal class LineToolOverlay : Control
{
    public static readonly DependencyProperty ZoomboxScaleProperty =
    DependencyProperty.Register(nameof(ZoomboxScale), typeof(double), typeof(LineToolOverlay),
         new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnZoomboxScaleChanged));

    public static readonly DependencyProperty LineStartProperty =
        DependencyProperty.Register(nameof(LineStart), typeof(VecD), typeof(LineToolOverlay),
            new FrameworkPropertyMetadata(VecD.Zero, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty LineEndProperty =
        DependencyProperty.Register(nameof(LineEnd), typeof(VecD), typeof(LineToolOverlay),
            new FrameworkPropertyMetadata(VecD.Zero, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ActionCompletedProperty =
        DependencyProperty.Register(nameof(ActionCompleted), typeof(ICommand), typeof(LineToolOverlay), new(null));

    public ICommand? ActionCompleted
    {
        get => (ICommand)GetValue(ActionCompletedProperty);
        set => SetValue(ActionCompletedProperty, value);
    }

    public VecD LineEnd
    {
        get => (VecD)GetValue(LineEndProperty);
        set => SetValue(LineEndProperty, value);
    }

    public VecD LineStart
    {
        get => (VecD)GetValue(LineStartProperty);
        set => SetValue(LineStartProperty, value);
    }

    public double ZoomboxScale
    {
        get => (double)GetValue(ZoomboxScaleProperty);
        set => SetValue(ZoomboxScaleProperty, value);
    }

    private Pen blackPen = new Pen(Brushes.Black, 1);

    private VecD mouseDownPos = VecD.Zero;
    private VecD lineStartOnMouseDown = VecD.Zero;
    private VecD lineEndOnMouseDown = VecD.Zero;

    private LineToolOverlayAnchor? capturedAnchor = null;
    private bool dragging = false;
    private bool movedWhileMouseDown = false;

    private PathGeometry handleGeometry = new()
    {
        FillRule = FillRule.Nonzero,
        Figures = (PathFigureCollection)new PathFigureCollectionConverter()
            .ConvertFrom("M 0.50025839 0 0.4248062 0.12971572 0.34987079 0.25994821 h 0.1002584 V 0.45012906 H 0.25994831 V 0.34987066 L 0.12971577 0.42480604 0 0.5002582 0.12971577 0.57519373 0.25994831 0.65012926 V 0.5498709 H 0.45012919 V 0.74005175 H 0.34987079 L 0.42480619 0.87028439 0.50025839 1 0.57519399 0.87028439 0.65012959 0.74005175 H 0.54987119 V 0.5498709 H 0.74005211 V 0.65012926 L 0.87028423 0.57519358 1 0.5002582 0.87028423 0.42480604 0.74005169 0.34987066 v 0.1002584 H 0.54987077 V 0.25994821 h 0.1002584 L 0.5751938 0.12971572 Z"),
    };

    private MouseUpdateController mouseUpdateController;

    public LineToolOverlay()
    {
        Cursor = Cursors.Arrow;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        mouseUpdateController = new MouseUpdateController(this, MouseMoved);
    }
    
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        mouseUpdateController?.Dispose();
    }

    private static void OnZoomboxScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        var self = (LineToolOverlay)obj;
        double newScale = (double)args.NewValue;
        self.blackPen.Thickness = 1.0 / newScale;
    }

    protected override void OnRender(DrawingContext context)
    {
        context.DrawRectangle(Brushes.White, blackPen, TransformHelper.ToAnchorRect(LineStart, ZoomboxScale));
        context.DrawRectangle(Brushes.White, blackPen, TransformHelper.ToAnchorRect(LineEnd, ZoomboxScale));

        VecD handlePos = TransformHelper.GetDragHandlePos(new ShapeCorners(new RectD(LineStart, LineEnd - LineStart)), ZoomboxScale);
        const double CrossSize = TransformHelper.MoveHandleSize - 1;
        context.DrawRectangle(Brushes.White, blackPen, TransformHelper.ToHandleRect(handlePos, ZoomboxScale));
        handleGeometry.Transform = new MatrixTransform(
            0, CrossSize / ZoomboxScale,
            CrossSize / ZoomboxScale, 0,
            handlePos.X - CrossSize / (ZoomboxScale * 2), handlePos.Y - CrossSize / (ZoomboxScale * 2)
        );
        context.DrawGeometry(Brushes.Black, null, handleGeometry);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.ChangedButton != MouseButton.Left)
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

        CaptureMouse();
    }

    protected void MouseMoved(object sender, MouseEventArgs e)
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

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.ChangedButton != MouseButton.Left)
            return;

        e.Handled = true;
        capturedAnchor = null;
        dragging = false;
        if (movedWhileMouseDown && ActionCompleted is not null && ActionCompleted.CanExecute(null))
            ActionCompleted.Execute(null);

        ReleaseMouseCapture();
    }
}
