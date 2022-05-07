using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.CustomControls.TransformOverlay;

internal class TransformOverlay : Control
{

    public static DependencyProperty AffineTransformProperty =
        DependencyProperty.Register(nameof(AffineTransform), typeof(AffineTransform), typeof(TransformOverlay),
            new FrameworkPropertyMetadata(default(AffineTransform), FrameworkPropertyMetadataOptions.AffectsRender));

    public static DependencyProperty PerspectiveTransformProperty =
        DependencyProperty.Register(nameof(PerspectiveTransform), typeof(ShapeCorners), typeof(TransformOverlay),
            new FrameworkPropertyMetadata(default(ShapeCorners), FrameworkPropertyMetadataOptions.AffectsRender));

    public static DependencyProperty ZoomboxScaleProperty =
        DependencyProperty.Register(nameof(ZoomboxScale), typeof(double), typeof(TransformOverlay),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register(nameof(Mode), typeof(TransformOverlayMode), typeof(TransformOverlay),
            new FrameworkPropertyMetadata(TransformOverlayMode.None, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnModeChange)));

    public TransformOverlayMode Mode
    {
        get => (TransformOverlayMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public AffineTransform AffineTransform
    {
        get => (AffineTransform)GetValue(AffineTransformProperty);
        set => SetValue(AffineTransformProperty, value);
    }

    public ShapeCorners PerspectiveTransform
    {
        get => (ShapeCorners)GetValue(PerspectiveTransformProperty);
        set => SetValue(PerspectiveTransformProperty, value);
    }

    public double ZoomboxScale
    {
        get => (double)GetValue(ZoomboxScaleProperty);
        set => SetValue(ZoomboxScaleProperty, value);
    }

    private ITransformMode? currentMode;

    public const double SideLength = 10;
    public Anchor? CapturedAnchor { get; private set; } = null;

    static TransformOverlay()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TransformOverlay), new FrameworkPropertyMetadata(typeof(TransformOverlay)));
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        if (Mode == TransformOverlayMode.None)
            return;
        currentMode?.OnRender(drawingContext);
    }

    private static void OnModeChange(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        var overlay = (TransformOverlay)obj;
        var value = (TransformOverlayMode)args.NewValue;
        overlay.ReleaseAnchor();
        overlay.currentMode = value switch
        {
            TransformOverlayMode.Affine => new AffineMode(overlay),
            TransformOverlayMode.Perspective => new PerpectiveMode(overlay),
            _ => null,
        };
        overlay.InvalidateVisual();
    }


    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (Mode == TransformOverlayMode.None || currentMode is null)
            return;

        var pos = ToVector2d(e.GetPosition(this));
        Anchor? anchor = currentMode.GetAnchorInPosition(pos);
        if (anchor is null)
            return;
        CapturedAnchor = anchor;
        CaptureMouse();
        e.Handled = true;
    }

    public bool IsWithinAnchor(Vector2d anchorPos, Vector2d mousePos)
    {
        return (anchorPos - mousePos).TaxicabLength <= (SideLength + 6) / ZoomboxScale / 2;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (CapturedAnchor is null || Mode == TransformOverlayMode.None || currentMode is null)
            return;
        e.Handled = true;
        var pos = ToVector2d(e.GetPosition(this));
        currentMode.OnAnchorDrag(pos, (Anchor)CapturedAnchor);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        if (ReleaseAnchor())
            e.Handled = true;
    }

    private bool ReleaseAnchor()
    {
        if (CapturedAnchor is null)
            return false;
        ReleaseMouseCapture();
        CapturedAnchor = null;
        return true;
    }

    public Rect ToRect(Vector2d pos)
    {
        double scaled = SideLength / ZoomboxScale;
        return new Rect(pos.X - scaled / 2, pos.Y - scaled / 2, scaled, scaled);
    }

    public static Vector2d ToVector2d(Point pos) => new Vector2d(pos.X, pos.Y);
    public static Point ToPoint(Vector2d vec) => new Point(vec.X, vec.Y);
}
