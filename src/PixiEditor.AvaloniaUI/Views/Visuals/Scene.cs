using System.Diagnostics;
using Avalonia;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using ChunkyImageLib;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
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

    public static readonly StyledProperty<double> AngleProperty = AvaloniaProperty.Register<Scene, double>(
        nameof(Angle), 0);

    public double Angle
    {
        get => GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

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

    private SKSurface _outputSurface;
    private SKPaint _paint = new SKPaint();
    private GRContext? gr;
    private RectI visibleSurfaceRect = new RectI(0, 0, 0, 0);

    static Scene()
    {
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
        CreateOutputSurface();
    }

    private void CreateOutputSurface()
    {
        if (gr == null) return;

        _outputSurface?.Dispose();
        GRGlFramebufferInfo frameBuffer = new GRGlFramebufferInfo(0, SKColorType.Rgba8888.ToGlSizedFormat());
        GRBackendRenderTarget desc = new GRBackendRenderTarget((int)Bounds.Width, (int)Bounds.Height, 4, 0, frameBuffer);
        _outputSurface = SKSurface.Create(gr, desc, GRSurfaceOrigin.BottomLeft, SKImageInfo.PlatformColorType);
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (Surface == null || Document == null) return;

        SKCanvas canvas = _outputSurface.Canvas;

        var scale = CalculateFinalScale();

        VecI surfaceStart = ViewportToSurface(new VecI(0, 0), scale, Angle);
        VecI surfaceEnd = ViewportToSurface(new VecI((int)Bounds.Width, (int)Bounds.Height), scale, Angle);

        if (IsOutOfBounds(surfaceStart, surfaceEnd))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.Flush();
            RequestNextFrameRendering();
            return;
        }

        canvas.Save();
        canvas.ClipRect(new SKRect(0, 0, (float)Bounds.Width, (float)Bounds.Height));

        canvas.Clear(SKColors.Transparent);

        canvas.RotateDegrees((float)Angle, ContentPosition.X, ContentPosition.Y);
        canvas.Scale(scale, scale, ContentPosition.X, ContentPosition.Y);
        canvas.Translate(ContentPosition.X, ContentPosition.Y);

        int x = Math.Max(0, surfaceStart.X);
        int y = Math.Max(0, surfaceStart.Y);
        int width = Math.Min(Surface.Size.X, surfaceEnd.X - x) + 1;
        int height = Math.Min(Surface.Size.Y, surfaceEnd.Y - y) + 1;

        visibleSurfaceRect = new RectI(x, y, width, height);

        using Image snapshot = Surface.DrawingSurface.Snapshot(visibleSurfaceRect);

        canvas.DrawImage((SKImage)snapshot.Native, visibleSurfaceRect.X, visibleSurfaceRect.Y, _paint);

        canvas.Restore();

        canvas.Flush();
        RequestNextFrameRendering();
    }

    private float CalculateFinalScale()
    {
        float scaleX = (float)Document.Width / Surface.Size.X;
        float scaleY = (float)Document.Height / Surface.Size.Y;
        var scaleUniform = Math.Min(scaleX, scaleY);

        float scale = (float)Scale * scaleUniform;
        return scale;
    }

    private bool IsOutOfBounds(VecI surfaceStart, VecI surfaceEnd)
    {
        return surfaceStart.X >= Surface.Size.X || surfaceStart.Y >= Surface.Size.Y || surfaceEnd.X <= 0 || surfaceEnd.Y <= 0;
    }

    private VecI ViewportToSurface(VecI point, float scale, double angle)
    {
        VecD nonRotated = new VecD(
         ((point.X - ContentPosition.X) / scale),
         ((point.Y - ContentPosition.Y) / scale));

        return new VecI((int)nonRotated.X, (int)nonRotated.Y);
        // TODO: Implement rotation and other matrix transformations
        /*double angleRad = angle * (Math.PI / 180);

        var rotated = nonRotated.Rotate(angleRad, ContentPosition);
        return new VecI((int)rotated.X, (int)rotated.Y);*/
    }

    private static void BoundsChanged(Scene sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Rect bounds)
        {
            sender.CreateOutputSurface();
        }
    }
}
