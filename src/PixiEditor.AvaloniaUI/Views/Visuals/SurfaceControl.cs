using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using ChunkyImageLib;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.DrawingApi.Skia;
using Colors = PixiEditor.DrawingApi.Core.ColorsImpl.Colors;
using Point = Avalonia.Point;

namespace PixiEditor.AvaloniaUI.Views.Visuals;

public class SurfaceControl : OpenGlControlBase
{
    public static readonly StyledProperty<Surface> SurfaceProperty = AvaloniaProperty.Register<SurfaceControl, Surface>(
        nameof(Surface));

    public static readonly StyledProperty<Stretch> StretchProperty = AvaloniaProperty.Register<SurfaceControl, Stretch>(
        nameof(Stretch), Stretch.Uniform);

    public static readonly StyledProperty<double> ScaleProperty = AvaloniaProperty.Register<SurfaceControl, double>(
        nameof(Scale), 1);


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

    public double Scale
    {
        get { return (double)GetValue(ScaleProperty); }
        set { SetValue(ScaleProperty, value); }
    }

    private DrawingSurfaceOp _drawingSurfaceOp;
    private GRContext grContext;
    private GRGlFramebufferInfo frameBuffer;
    private SKSurface surface;

    static SurfaceControl()
    {
        AffectsRender<SurfaceControl>(StretchProperty);
        SurfaceProperty.Changed.AddClassHandler<SurfaceControl>(OnSurfaceChanged);
        BoundsProperty.Changed.AddClassHandler<SurfaceControl>(BoundsChanged);
        StretchProperty.Changed.AddClassHandler<SurfaceControl>(StretchChanged);
        WidthProperty.Changed.AddClassHandler<SurfaceControl>(BoundsChanged);
        HeightProperty.Changed.AddClassHandler<SurfaceControl>(BoundsChanged);
    }

    /*public override void Render(DrawingContext context)
    {
        if (Surface == null)
        {
            return;
        }

        context.Custom(_drawingSurfaceOp);
    }*/

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);
        if (DrawingBackendApi.Current is SkiaDrawingBackend skiaDrawingBackend)
        {
            grContext = GRContext.CreateGl(GRGlInterface.Create(gl.GetProcAddress));
            skiaDrawingBackend.SurfaceImplementation.SetGrContext(grContext);
        }

        grContext = GRContext.CreateGl(GRGlInterface.Create(gl.GetProcAddress));
                SKImage snapshot = ((SKSurface)Surface.DrawingSurface.Native).Snapshot();
        frameBuffer = new GRGlFramebufferInfo(0, SKColorType.Rgba8888.ToGlSizedFormat());
        GRBackendRenderTarget desc = new GRBackendRenderTarget((int)Bounds.Width, (int)Bounds.Height, 4, 0, frameBuffer);

        surface = SKSurface.Create(grContext, desc, GRSurfaceOrigin.BottomLeft, snapshot.ColorType);
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (Surface == null)
        {
            return;
        }

        SKCanvas canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        using (var paint = new SKPaint())
        {
            canvas.DrawSurface((SKSurface)Surface.DrawingSurface.Native, 0, 0, paint);
        }

        canvas.Flush();

        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
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

    //TODO: Implement dirty rect handling
    /*private RectI? _lastDirtyRect;
    private SKImage? _lastImage;
    private SKImage? _lastFullImage;*/

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
            /*if(_lastDirtyRect != Surface.DirtyRect)
            {
                RectI dirtyRect = Surface.DirtyRect;
                if (dirtyRect.IsZeroOrNegativeArea)
                {
                    dirtyRect = new RectI(0, 0, Surface.Size.X, Surface.Size.Y);
                    _lastFullImage = (SKImage)Surface.DrawingSurface.Snapshot().Native;
                }

                _lastImage = (SKImage)Surface.DrawingSurface.Snapshot(dirtyRect).Native;
                _lastDirtyRect = Surface.DirtyRect;
            }

            if (_lastFullImage != null)
            {
                canvas.DrawImage(_lastFullImage, new SKPoint(0, 0), _paint);
            }

            if (_lastImage != null)
            {
                canvas.DrawImage(_lastImage, new SKPoint(_lastDirtyRect.Value.X, _lastDirtyRect.Value.Y), _paint);
            }*/

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
