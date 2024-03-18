using Avalonia;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using ChunkyImageLib;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Skia;

namespace PixiEditor.AvaloniaUI.Views.Visuals;

internal class Scene : OpenGlControlBase
{
    public static readonly StyledProperty<Surface> SurfaceProperty = AvaloniaProperty.Register<SurfaceControl, Surface>(
        nameof(Surface));

    public static readonly StyledProperty<double> ScaleProperty = AvaloniaProperty.Register<Scene, double>(
        nameof(Scale), 1);

    public static readonly StyledProperty<VecI> ContentPositionProperty = AvaloniaProperty.Register<Scene, VecI>(
        nameof(ContentPosition));

    public static readonly StyledProperty<DocumentViewModel> DocumentProperty = AvaloniaProperty.Register<Scene, DocumentViewModel>(
        nameof(Document));

    public DocumentViewModel Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public VecI ContentPosition
    {
        get => GetValue(ContentPositionProperty);
        set => SetValue(ContentPositionProperty, value);
    }

    public double Scale
    {
        get => GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public Surface Surface
    {
        get => GetValue(SurfaceProperty);
        set => SetValue(SurfaceProperty, value);
    }

    private SKSurface _workingSurface;
    private SKSurface _viewportSizedSurface;
    private SKPaint _paint = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
    private GRContext? gr;

    static Scene()
    {
        SurfaceProperty.Changed.AddClassHandler<Scene>(OnSurfaceChanged);
        BoundsProperty.Changed.AddClassHandler<Scene>(BoundsChanged);
        WidthProperty.Changed.AddClassHandler<Scene>(BoundsChanged);
        HeightProperty.Changed.AddClassHandler<Scene>(BoundsChanged);
    }

    public Scene()
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
        SKCanvas canvas = _workingSurface.Canvas;
        canvas.Save();
        canvas.ClipRect(new SKRect(0, 0, (float)Bounds.Width, (float)Bounds.Height));

        canvas.Clear(SKColors.Transparent);

        float scaleX = (float)Document.Width / Surface.Size.X;
        float scaleY = (float)Document.Height / Surface.Size.Y;
        var scaleUniform = Math.Min(scaleX, scaleY);

        float scale = (float)Scale * scaleUniform;

        canvas.Scale(scale, scale, ContentPosition.X, ContentPosition.Y);
        canvas.Translate(ContentPosition.X, ContentPosition.Y);

        //canvas.Translate((float)Bounds.Width / 2f - Surface.Size.X / 2f, (float)Bounds.Height / 2f - Surface.Size.Y / 2f);

        canvas.DrawSurface((SKSurface)Surface.DrawingSurface.Native, 0, 0, _paint);
        //canvas.DrawRect(0, 0, Surface.Size.X, Surface.Size.Y, _paint);

        canvas.Restore();

        canvas.Flush();
        RequestNextFrameRendering();
    }

    private static void StretchChanged(Scene sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Stretch stretch)
        {
            //sender._drawingSurfaceOp = new DrawingSurfaceOp(sender.Surface, sender.Bounds, stretch);
        }
    }

    private static void BoundsChanged(Scene sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Rect bounds)
        {
            //sender._drawingSurfaceOp = new DrawingSurfaceOp(sender.Surface, bounds, sender.Stretch);
            sender.CreateWorkingSurface();
        }
    }

    private static void OnSurfaceChanged(Scene sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Surface surface)
        {
            //sender._drawingSurfaceOp = new DrawingSurfaceOp(surface, sender.Bounds, sender.Stretch);
        }
    }
}
