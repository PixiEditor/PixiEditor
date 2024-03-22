using Avalonia;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using ChunkyImageLib;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Visuals;

public class SurfaceControl : OpenGlControlBase
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

    private SKSurface _workingSurface;
    private SKPaint _paint = new SKPaint() { BlendMode = SKBlendMode.Src };
    private GRContext? gr;
    private RectI? nextDirtyRect;

    static SurfaceControl()
    {
        AffectsRender<SurfaceControl>(StretchProperty, SurfaceProperty);
        BoundsProperty.Changed.AddClassHandler<SurfaceControl>(BoundsChanged);
        SurfaceProperty.Changed.AddClassHandler<SurfaceControl>(Rerender);
        StretchProperty.Changed.AddClassHandler<SurfaceControl>(Rerender);
    }

    public SurfaceControl()
    {
        ClipToBounds = true;
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        gr = GRContext.CreateGl(GRGlInterface.Create(gl.GetProcAddress));
        CreateWorkingSurface();
    }

    private void CreateWorkingSurface()
    {
        if (gr == null) return;

        _workingSurface?.Dispose();
        GRGlFramebufferInfo frameBuffer = new GRGlFramebufferInfo(0, SKColorType.Rgba8888.ToGlSizedFormat());
        GRBackendRenderTarget desc = new GRBackendRenderTarget((int)Bounds.Width, (int)Bounds.Height, 4, 0, frameBuffer);
        _workingSurface = SKSurface.Create(gr, desc, GRSurfaceOrigin.BottomLeft, SKImageInfo.PlatformColorType);
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (Surface == null)
        {
            return;
        }

        SKCanvas canvas = _workingSurface.Canvas;
        canvas.Save();
        canvas.ClipRect(new SKRect(0, 0, (float)Bounds.Width, (float)Bounds.Height));
        ScaleCanvas(canvas);
        canvas.Clear(SKColors.Transparent);

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

        canvas.DrawSurface((SKSurface)Surface.DrawingSurface.Native, new SKPoint(0, 0), _paint);

        canvas.Restore();
        canvas.Flush();
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

        Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Render);
    }

    private static void BoundsChanged(SurfaceControl sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Rect bounds)
        {
            sender.CreateWorkingSurface();
        }

        Dispatcher.UIThread.Post(sender.RequestNextFrameRendering, DispatcherPriority.Render);
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

        Dispatcher.UIThread.Post(sender.RequestNextFrameRendering, DispatcherPriority.Render);
    }
}
