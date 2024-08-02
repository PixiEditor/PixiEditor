using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Skia.Implementations;
using PixiEditor.Numerics;
using SkiaSharp;

namespace InjectedDrawingApiAvalonia.Visuals;

internal class SurfaceControl : Control
{
    public static readonly StyledProperty<DrawingSurface> SurfaceProperty =
        AvaloniaProperty.Register<SurfaceControl, DrawingSurface>(
            nameof(Surface));

    public static readonly StyledProperty<Stretch> StretchProperty = AvaloniaProperty.Register<SurfaceControl, Stretch>(
        nameof(Stretch), Stretch.Uniform);

    public static readonly StyledProperty<IBrush> BackgroundProperty =
        AvaloniaProperty.Register<SurfaceControl, IBrush>(
            nameof(Background));

    public IBrush Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public DrawingSurface Surface
    {
        get => GetValue(SurfaceProperty);
        set => SetValue(SurfaceProperty, value);
    }

    public Action<SKCanvas> Draw { get; set; }

    private RectI? nextDirtyRect;

    static SurfaceControl()
    {
        AffectsMeasure<SurfaceControl>(StretchProperty, SurfaceProperty);
        BoundsProperty.Changed.AddClassHandler<SurfaceControl>(BoundsChanged);
        SurfaceProperty.Changed.AddClassHandler<SurfaceControl>(Rerender);
        StretchProperty.Changed.AddClassHandler<SurfaceControl>(Rerender);
    }

    public SurfaceControl()
    {
        ClipToBounds = true;
    }

    /// <summary>
    /// Measures the control.
    /// </summary>
    /// <param name="availableSize">The available size.</param>
    /// <returns>The desired size of the control.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        var source = Surface;
        var result = new Size();

        result = new Size(Width, Height); 

        return result;
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var source = Surface;
        return finalSize;
    }

    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(Bounds.Size);
        var operation = new DrawSurfaceOperation(Draw, bounds, Stretch, Opacity);
        context.Custom(operation);
        
        Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
    }

    private void SurfaceChanged(RectD? changedRect)
    {
        if (changedRect.HasValue)
        {
            var rect = changedRect.Value;
            var rectI = new RectI((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            nextDirtyRect = nextDirtyRect?.Union(rectI) ?? rectI;
        }
        else
        {
            nextDirtyRect = null;
        }

        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Render);
    }

    private static void BoundsChanged(SurfaceControl sender, AvaloniaPropertyChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(sender.InvalidateVisual, DispatcherPriority.Render);
    }

    private static void Rerender(SurfaceControl sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is DrawingSurface oldSurface)
        {
            oldSurface.Changed -= sender.SurfaceChanged;
        }

        if (e.NewValue is DrawingSurface newSurface)
        {
            newSurface.Changed += sender.SurfaceChanged;
        }

        Dispatcher.UIThread.Post(sender.InvalidateVisual, DispatcherPriority.Render);
    }
}

internal class DrawSurfaceOperation : SkiaDrawOperation
{
    public Stretch Stretch { get; }

    public double Opacity { get; set; } = 1.0;
    
    public Action<SKCanvas> Draw;

    private SKPaint _paint = new SKPaint();

    public DrawSurfaceOperation(Action<SKCanvas> drawEvent, Rect dirtyBounds, Stretch stretch, double opacity = 1) :
        base(dirtyBounds)
    {
        Stretch = stretch;
        Opacity = opacity;
        Draw = drawEvent;
    }

    public override void Render(ISkiaSharpApiLease lease)
    {
        SKCanvas canvas = lease.SkCanvas;

        (DrawingBackendApi.Current.SurfaceImplementation as SkiaSurfaceImplementation).GrContext = lease.GrContext;
            
        canvas.Save();
        _paint.Color = _paint.Color.WithAlpha((byte)(Opacity * 255));
        Draw?.Invoke(canvas);
        canvas.Restore();
    }


    public override bool Equals(ICustomDrawOperation? other)
    {
        return other is DrawSurfaceOperation otherOp && otherOp.Stretch == Stretch;
    }

    public override void Dispose()
    {
        _paint.Dispose();
    }
}
