using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using PixiEditor.DrawingApi.Core.Surface;
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
        if (Background != null)
        {
            context.FillRectangle(Background, new Rect(0, 0, Bounds.Width, Bounds.Height));
        }

        if (Surface == null || Surface.IsDisposed)
        {
            return;
        }

        var bounds = new Rect(Bounds.Size);
        var operation = new DrawSurfaceOperation(bounds, Surface, Stretch, Opacity);
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
    public DrawingSurface Surface { get; }
    public Stretch Stretch { get; }

    public double Opacity { get; set; } = 1.0;

    private SKPaint _paint = new SKPaint();
    
    private WriteableBitmap targetBitmap;

    public DrawSurfaceOperation(Rect dirtyBounds, DrawingSurface surface, Stretch stretch, double opacity = 1) :
        base(dirtyBounds)
    {
        Surface = surface;
        Stretch = stretch;
        Opacity = opacity;
    }

    public override void Render(ISkiaSharpApiLease lease)
    {
        SKCanvas canvas = lease.SkCanvas;

        if (Surface == null || Surface.IsDisposed)
        {
            return;
        }
            
        canvas.Save();
        _paint.Color = _paint.Color.WithAlpha((byte)(Opacity * 255));
        canvas.DrawSurface((SKSurface)Surface.Native, new SKPoint(0, 0), _paint);
        canvas.Restore();
    }


    public override bool Equals(ICustomDrawOperation? other)
    {
        return other is DrawSurfaceOperation otherOp && otherOp.Surface == Surface && otherOp.Stretch == Stretch;
    }

    public override void Dispose()
    {
        _paint.Dispose();
    }
}
