﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using ChunkyImageLib;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Visuals;

internal class SurfaceControl : Control
{
    public static readonly StyledProperty<Surface> SurfaceProperty = AvaloniaProperty.Register<SurfaceControl, Surface>(
        nameof(Surface));

    public static readonly StyledProperty<Stretch> StretchProperty = AvaloniaProperty.Register<SurfaceControl, Stretch>(
        nameof(Stretch), Stretch.Uniform);

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public Surface Surface
    {
        get => GetValue(SurfaceProperty);
        set => SetValue(SurfaceProperty, value);
    }

    private RectI? nextDirtyRect;

    static SurfaceControl()
    {
        AffectsRender<SurfaceControl>(StretchProperty, SurfaceProperty);
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

        if (source != null)
        {
            result = Stretch.CalculateSize(availableSize, new Size(source.Size.X, source.Size.Y));
        }

        return result;
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var source = Surface;

        if (source != null)
        {
            var sourceSize = source.Size;
            var result = Stretch.CalculateSize(finalSize, new Size(sourceSize.X, sourceSize.Y));
            return result;
        }

        return new Size();
    }

    public override void Render(DrawingContext context)
    {
        if (Surface == null)
        {
            return;
        }

        var bounds = new Rect(Bounds.Size);
        var operation = new DrawSurfaceOperation(bounds, Surface, Stretch, Opacity);
        context.Custom(operation);
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
        if (e.OldValue is Surface oldSurface)
        {
            oldSurface.Changed -= sender.SurfaceChanged;
        }
        if (e.NewValue is Surface newSurface)
        {
            newSurface.Changed += sender.SurfaceChanged;
        }

        Dispatcher.UIThread.Post(sender.InvalidateVisual, DispatcherPriority.Render);
    }
}

internal class DrawSurfaceOperation : SkiaDrawOperation
{
    public Surface Surface { get; }
    public Stretch Stretch { get; }

    public double Opacity { get; set; } = 1.0;

    private SKPaint _paint = new SKPaint();

    public DrawSurfaceOperation(Rect dirtyBounds, Surface surface, Stretch stretch, double opacity = 1) : base(dirtyBounds)
    {
        Surface = surface;
        Stretch = stretch;
        Opacity = opacity;
    }

    public override void Render(ISkiaSharpApiLease lease)
    {
        SKCanvas canvas = lease.SkCanvas;
        if (Surface == null)
        {
            return;
        }

        canvas.Save();
        ScaleCanvas(canvas);

        //TODO: Implement dirty rect rendering
        /*if (nextDirtyRect.HasValue)
        {
            using SKImage snapshot = (SKImage)Surface.DrawingSurface.Snapshot(nextDirtyRect.Value).Native;
            canvas.DrawImage(snapshot, nextDirtyRect.Value.X, nextDirtyRect.Value.Y, _paint);
        }
        else
        {
            canvas.DrawSurface((SKSurface)Surface.DrawingSurface.Native, new SKPoint(0, 0), _paint);
        }*/

        _paint.Color = _paint.Color.WithAlpha((byte)(Opacity * 255));
        canvas.DrawSurface((SKSurface)Surface.DrawingSurface.Native, new SKPoint(0, 0), _paint);
        canvas.Restore();
    }


    private void ScaleCanvas(SKCanvas canvas)
    {
        if (Stretch == Stretch.Fill)
        {
            canvas.Scale((float)Bounds.Width / Surface.Size.X, (float)Bounds.Height / Surface.Size.Y);
        }
        else if (Stretch == Stretch.Uniform)
        {
            float scaleX = (float)Bounds.Width / Surface.Size.X;
            float scaleY = (float)Bounds.Height / Surface.Size.Y;
            var scale = Math.Min(scaleX, scaleY);
            float dX = (float)Bounds.Width / 2 / scale - Surface.Size.X / 2;
            float dY = (float)Bounds.Height / 2 / scale - Surface.Size.Y / 2;
            canvas.Scale(scale, scale);
            canvas.Translate(dX, dY);
        }
        else if (Stretch == Stretch.UniformToFill)
        {
            float scaleX = (float)Bounds.Width / Surface.Size.X;
            float scaleY = (float)Bounds.Height / Surface.Size.Y;
            var scale = Math.Max(scaleX, scaleY);
            float dX = (float)Bounds.Width / 2 / scale - Surface.Size.X / 2;
            float dY = (float)Bounds.Height / 2 / scale - Surface.Size.Y / 2;
            canvas.Scale(scale, scale);
            canvas.Translate(dX, dY);
        }
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