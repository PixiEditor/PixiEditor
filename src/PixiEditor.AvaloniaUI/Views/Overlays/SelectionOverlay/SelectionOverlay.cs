using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Styling;
using PixiEditor.AvaloniaUI.Animation;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.Vector;

namespace PixiEditor.AvaloniaUI.Views.Overlays.SelectionOverlay;
#nullable enable
internal class SelectionOverlay : Overlay
{
    public static readonly StyledProperty<VectorPath?> PathProperty =
        AvaloniaProperty.Register<SelectionOverlay, VectorPath?>(nameof(Path));

    public VectorPath? Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
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
        ShowFillProperty.Changed.Subscribe(OnShowFillChanged);
        PathProperty.Changed.Subscribe(OnPathChanged);
    }

    private Pen whitePen = new Pen(Brushes.White, 1);
    private Pen blackDashedPen = new Pen(Brushes.Black, 1) { DashStyle = startingFrame };
    private Brush fillBrush = new SolidColorBrush(Color.FromArgb(80, 0, 80, 255));

    private static DashStyle startingFrame = new DashStyle(new double[] { 2, 4 }, 0);

    private Geometry renderPath = new PathGeometry();
    private Avalonia.Animation.Animation animation;
    private CancellationTokenSource cancelAnimationToken;

    public SelectionOverlay()
    {
        IsHitTestVisible = false;

        animation = new Avalonia.Animation.Animation()
        {
            Duration = new TimeSpan(0, 0, 0, 2, 0),
            IterationCount = IterationCount.Infinite,
        };

        int steps = 7;
        float step = 1f / steps;

        for (int i = 0; i < steps; i++)
        {
            Cue cue = new Cue(i * step);
            animation.Children.Add(new KeyFrame()
            {
                Cue = cue,
                Setters = { new Setter(BlackDashedPenProperty, SelectionDashAnimator.Interpolate(cue.CueValue, 6, blackDashedPen.DashStyle.Dashes.ToArray())) }
            });
        }
    }

    public override void RenderOverlay(DrawingContext context, RectD canvasBounds)
    {
        base.Render(context);
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
        context.DrawGeometry(null, whitePen, renderPath);
        context.DrawGeometry(fillBrush, blackDashedPen, renderPath);
    }

    protected override void ZoomChanged(double newZoom)
    {
        whitePen.Thickness = 1.0 / newZoom;
        blackDashedPen.Thickness = 1.0 / newZoom;
    }

    private static void OnShowFillChanged(AvaloniaPropertyChangedEventArgs<bool> args)
    {
        var self = (SelectionOverlay)args.Sender;
        self.fillBrush.Opacity = args.NewValue.Value ? 1 : 0;
    }

    private static void OnPathChanged(AvaloniaPropertyChangedEventArgs<VectorPath?> args)
    {
        var self = (SelectionOverlay)args.Sender;
        if (args.NewValue.HasValue && !args.NewValue.Value.IsEmpty && self.IsVisible)
        {
            self.cancelAnimationToken = new CancellationTokenSource();
            self.animation.RunAsync(self, self.cancelAnimationToken.Token);
        }
        else if (self.cancelAnimationToken != null)
        {
            self.cancelAnimationToken.Cancel();
        }
    }
}
