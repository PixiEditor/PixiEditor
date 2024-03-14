using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using ChunkyImageLib;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using Image = PixiEditor.DrawingApi.Core.Surface.ImageData.Image;
using Point = Avalonia.Point;

namespace PixiEditor.AvaloniaUI.Views.Visuals;

public class SurfaceControl : Control
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

    private DrawingSurfaceOp _drawingSurfaceOp;

    static SurfaceControl()
    {
        AffectsRender<SurfaceControl>(SurfaceProperty, StretchProperty);
        SurfaceProperty.Changed.AddClassHandler<SurfaceControl>(OnSurfaceChanged);
        BoundsProperty.Changed.AddClassHandler<SurfaceControl>(BoundsChanged);
        StretchProperty.Changed.AddClassHandler<SurfaceControl>(StretchChanged);
    }

    public override void Render(DrawingContext context)
    {
        if (Surface == null)
        {
            return;
        }

        context.Custom(_drawingSurfaceOp);
    }

    private static void StretchChanged(SurfaceControl sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Stretch stretch)
        {
            sender._drawingSurfaceOp = new DrawingSurfaceOp(sender.Surface, sender.Bounds, stretch);
        }
    }

    private static void BoundsChanged(SurfaceControl sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Rect bounds)
        {
            sender._drawingSurfaceOp = new DrawingSurfaceOp(sender.Surface, bounds, sender.Stretch);
        }
    }

    private static void OnSurfaceChanged(SurfaceControl sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Surface surface)
        {
            sender._drawingSurfaceOp = new DrawingSurfaceOp(surface, sender.Bounds, sender.Stretch);
        }
    }
}

public class DrawingSurfaceOp : ICustomDrawOperation
{
    public Rect Bounds { get;  }
    public Surface Surface { get; }
    public Stretch Stretch { get; set; }

    private SKPaint _paint = new SKPaint();

    public DrawingSurfaceOp(Surface surface, Rect bounds, Stretch stretch)
    {
        Surface = surface;
        Bounds = bounds;
        Stretch = stretch;
    }

    public void Render(ImmediateDrawingContext context)
    {
        if (context.TryGetFeature(out ISkiaSharpApiLeaseFeature skiaSurface))
        {
            using var lease = skiaSurface.Lease();
            var canvas = lease.SkCanvas;
            canvas.Save();
            ScaleCanvas(canvas);
            canvas.DrawSurface((SKSurface)Surface.DrawingSurface.Native, 0, 0, _paint);
            canvas.Restore();
        }
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

    public bool HitTest(Point p)
    {
        return false;
    }

    public bool Equals(ICustomDrawOperation? other)
    {
        return other is DrawingSurfaceOp op && op.Surface == Surface;
    }

    public void Dispose()
    {

    }
}
