using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Styling;
using PixiEditor.AvaloniaUI.Animators;
using PixiEditor.DrawingApi.Core.Surface.Vector;

namespace PixiEditor.Views.UserControls.Overlays;
#nullable enable
internal class SelectionOverlay : Control
{
    public static readonly StyledProperty<VectorPath?> PathProperty =
        AvaloniaProperty.Register<SelectionOverlay, VectorPath?>(nameof(Path));

    public VectorPath? Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public static readonly StyledProperty<double> ZoomboxScaleProperty =
        AvaloniaProperty.Register<SelectionOverlay, double>(nameof(ZoomboxScale), defaultValue: 1.0);

    public double ZoomboxScale
    {
        get => GetValue(ZoomboxScaleProperty);
        set => SetValue(ZoomboxScaleProperty, value);
    }

    public static readonly StyledProperty<bool> ShowFillProperty =
        AvaloniaProperty.Register<SelectionOverlay, bool>(nameof(ShowFill), defaultValue: true);

    public bool ShowFill
    {
        get => GetValue(ShowFillProperty);
        set => SetValue(ShowFillProperty, value);
    }

    public static readonly DirectProperty<SelectionOverlay, IDashStyle> BlackDashedPenProperty =
        AvaloniaProperty.RegisterDirect<SelectionOverlay, IDashStyle>("BlackDashedPen",
            overlay => overlay.blackDashedPen.DashStyle,
            (overlay, pen) => overlay.blackDashedPen.DashStyle = pen);

    static SelectionOverlay()
    {
        AffectsRender<SelectionOverlay>(PathProperty);
        ZoomboxScaleProperty.Changed.Subscribe(OnZoomboxScaleChanged);
        ShowFillProperty.Changed.Subscribe(OnShowFillChanged);
    }

    private Pen whitePen = new Pen(Brushes.White, 1);
    private Pen blackDashedPen = new Pen(Brushes.Black, 1) { DashStyle = startingFrame };
    private Brush fillBrush = new SolidColorBrush(Color.FromArgb(80, 0, 80, 255));

    private static DashStyle startingFrame = new DashStyle(new double[] { 2, 4 }, 6);

    private Geometry renderPath = new PathGeometry();

    public SelectionOverlay()
    {
        IsHitTestVisible = false;

        Animation animation = new Animation()
        {
            Duration = new TimeSpan(0, 0, 0, 2, 0),
            IterationCount = IterationCount.Infinite,
        };

        float step = 1f / 7f;

        for (int i = 0; i < 7; i++)
        {
            Cue cue = new Cue(i * step);
            animation.Children.Add(new KeyFrame()
            {
                Cue = cue,
                Setters = { new Setter(BlackDashedPenProperty, SelectionDashAnimator.Interpolate(cue.CueValue)) }
            });
        }

        animation.RunAsync(this);
    }

    public override void Render(DrawingContext drawingContext)
    {
        base.Render(drawingContext);
        if (Path is null)
            return;

        try
        {
            renderPath = new PathGeometry()
            {
                FillRule = FillRule.EvenOdd,
                Figures = (PathFigures?)PathFigures.Parse(Path.ToSvgPathData()),
            };
        }
        catch (FormatException)
        {
            return;
        }
        drawingContext.DrawGeometry(null, whitePen, renderPath);
        drawingContext.DrawGeometry(fillBrush, blackDashedPen, renderPath);
    }

    private static void OnZoomboxScaleChanged(AvaloniaPropertyChangedEventArgs<double> args)
    {
        var self = (SelectionOverlay)args.Sender;
        double newScale = args.NewValue.Value;
        self.whitePen.Thickness = 1.0 / newScale;
        self.blackDashedPen.Thickness = 1.0 / newScale;
    }

    private static void OnShowFillChanged(AvaloniaPropertyChangedEventArgs<bool> args)
    {
        var self = (SelectionOverlay)args.Sender;
        self.fillBrush.Opacity = args.NewValue.Value ? 1 : 0;
    }
}
