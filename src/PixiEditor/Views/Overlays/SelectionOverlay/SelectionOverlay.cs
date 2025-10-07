using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Styling;
using PixiEditor.Animation;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.Helpers.Extensions;
using Color = Drawie.Backend.Core.ColorsImpl.Color;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace PixiEditor.Views.Overlays.SelectionOverlay;
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

    public static readonly DirectProperty<SelectionOverlay, IDashPathEffect> BlackDashedPenProperty =
        AvaloniaProperty.RegisterDirect<SelectionOverlay, IDashPathEffect>("BlackDashedPen",
            overlay => overlay.blackDashedPenEffect,
            (overlay, pen) =>
            {
                overlay.blackDashedPenEffect = pen;
                try
                {
                    overlay.blackDashedPen.PathEffect = pen.ToPathEffect();
                }
                catch
                {
                    
                }
            });

    static SelectionOverlay()
    {
        AffectsRender<SelectionOverlay>(PathProperty);
        PathProperty.Changed.Subscribe(OnPathChanged);
    }

    private Paint whitePaint = new Paint() { Color = Colors.White, IsAntiAliased = true, StrokeWidth = 1, Style = PaintStyle.Stroke };

    private Paint blackDashedPen = new Paint
    {
        Color = Colors.Black, StrokeWidth = 1, IsAntiAliased = true, Style = PaintStyle.Stroke,
    };
    
    private IDashPathEffect blackDashedPenEffect = new DashPathEffect(new float[] { 2, 4 }, 0);

    private Paint fillBrush = new Paint() { Color = Color.FromArgb(80, 0, 80, 255), IsAntiAliased = true, Style = PaintStyle.Fill };

    private static DashPathEffect startingFrame;

    private VectorPath renderPath; 
    private Avalonia.Animation.Animation animation;
    private CancellationTokenSource cancelAnimationToken;
    private bool isAnimating;

    public SelectionOverlay()
    {
        IsHitTestVisible = false;

        if (Application.Current.Styles.TryGetResource("SelectionFillBrush", Application.Current.ActualThemeVariant,
                out var fillBrush))
        {
            if (fillBrush is SolidColorBrush solidColorBrush)
            {
                this.fillBrush = new Paint() { Color = solidColorBrush.Color.ToColor(), IsAntiAliased = true };
            }
        }

        blackDashedPen.PathEffect = blackDashedPenEffect.ToPathEffect();

        animation = new Avalonia.Animation.Animation()
        {
            Duration = new TimeSpan(0, 0, 0, 2, 0), IterationCount = IterationCount.Infinite,
        };
        
        int steps = 7;
        float step = 1f / steps;

        for (int i = 0; i < steps; i++)
        {
            Cue cue = new Cue(i * step);
            Pen pen;
            animation.Children.Add(new KeyFrame()
            {
                Cue = cue,
                Setters =
                {
                    new Setter(BlackDashedPenProperty, SelectionDashAnimator.Interpolate(cue.CueValue, 6, blackDashedPenEffect.Dashes.ToArray()))
                }
            });
        }
    }

    protected override void OnRenderOverlay(Canvas context, RectD canvasBounds)
    {
        if (isAnimating)
        {
            Refresh();
        }
        
        if (Path is null)
            return;

        try
        {
            renderPath = new VectorPath(Path);
            renderPath.FillType = PathFillType.EvenOdd;
        }
        catch (FormatException)
        {
            return;
        }

        context.DrawPath(renderPath, whitePaint);
        if (ShowFill)
        {
            context.DrawPath(renderPath, fillBrush);
        }

        blackDashedPen.PathEffect?.Dispose();
        float finalScale = Math.Min((float)ZoomScale, 20f);
        blackDashedPen.PathEffect = PathEffect.CreateDash(blackDashedPenEffect.Dashes.Select(d => d / finalScale).ToArray(), blackDashedPenEffect.Phase);
        context.DrawPath(renderPath, blackDashedPen);
    }

    protected override void ZoomChanged(double newZoom)
    {
        whitePaint.StrokeWidth = 1.0f / (float)newZoom;
        blackDashedPen.StrokeWidth = 1.0f / (float)newZoom;
    }

    private static void OnPathChanged(AvaloniaPropertyChangedEventArgs<VectorPath?> args)
    {
        var self = (SelectionOverlay)args.Sender;
        if (args.NewValue is { HasValue: true, Value.IsEmpty: false } && self.IsVisible)
        {
            self.cancelAnimationToken = new CancellationTokenSource();
            self.animation.RunAsync(self, self.cancelAnimationToken.Token);
            self.isAnimating = true;
            self.Refresh();
        }
        else if (self.cancelAnimationToken != null)
        {
            self.isAnimating = false;
            self.cancelAnimationToken.Cancel();
        }
    }
}
