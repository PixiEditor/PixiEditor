using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

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

    public static readonly StyledProperty<bool> FlipXProperty = AvaloniaProperty.Register<Scene, bool>(
        nameof(FlipX), false);
    public static readonly StyledProperty<bool> FlipYProperty = AvaloniaProperty.Register<Scene, bool>(
        nameof(FlipY), false);

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

    public Rect FinalBounds => Bounds;

    public bool FlipX
    {
        get { return (bool)GetValue(FlipXProperty); }
        set { SetValue(FlipXProperty, value); }
    }

    public bool FlipY
    {
        get { return (bool)GetValue(FlipYProperty); }
        set { SetValue(FlipYProperty, value); }
    }

    private SKSurface _outputSurface;
    private SKPaint _paint = new SKPaint();
    private GRContext? gr;

    static Scene()
    {
        AffectsRender<Scene>(BoundsProperty, WidthProperty, HeightProperty, ScaleProperty, AngleProperty, FlipXProperty, FlipYProperty, ContentPositionProperty, DocumentProperty, SurfaceProperty);
        BoundsProperty.Changed.AddClassHandler<Scene>(BoundsChanged);
        FlipXProperty.Changed.AddClassHandler<Scene>(RequestRendering);
        FlipYProperty.Changed.AddClassHandler<Scene>(RequestRendering);
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

        canvas.Save();
        canvas.ClipRect(new SKRect(0, 0, (float)Bounds.Width, (float)Bounds.Height));
        canvas.Clear(SKColors.Transparent);

        float finalScale = CalculateFinalScale();

        RectI surfaceRectToRender = FindRectToRender(finalScale);

        if (surfaceRectToRender.IsZeroOrNegativeArea)
        {
            canvas.Restore();
            canvas.Flush();
            RequestNextFrameRendering();
            return;
        }

        canvas.Scale(finalScale, finalScale, ContentPosition.X, ContentPosition.Y);
        float angle = (float)Angle;
        if (FlipX)
        {
            angle = 360 - angle;
        }
        if (FlipY)
        {
            angle = 360 - angle;
        }

        canvas.RotateDegrees(angle, ContentPosition.X, ContentPosition.Y);
        canvas.Scale(FlipX ? -1 : 1, FlipY ? -1 : 1, ContentPosition.X, ContentPosition.Y);
        canvas.Translate(ContentPosition.X, ContentPosition.Y);

        using Image snapshot = Surface.DrawingSurface.Snapshot(surfaceRectToRender);
        canvas.DrawImage((SKImage)snapshot.Native, surfaceRectToRender.X, surfaceRectToRender.Y, _paint);

        canvas.Restore();

        canvas.Flush();
    }

    private RectI FindRectToRender(float finalScale)
    {
        ShapeCorners surfaceInViewportSpace = SurfaceToViewport(new RectI(VecI.Zero, Surface.Size), finalScale);
        RectI surfaceBoundsInViewportSpace = (RectI)surfaceInViewportSpace.AABBBounds.RoundOutwards();
        RectI viewportBoundsInViewportSpace = (RectI)(new RectD(FinalBounds.X, FinalBounds.Y, FinalBounds.Width, FinalBounds.Height)).RoundOutwards();
        RectI firstIntersectionInViewportSpace = surfaceBoundsInViewportSpace.Intersect(viewportBoundsInViewportSpace);
        ShapeCorners firstIntersectionInSurfaceSpace = ViewportToSurface(firstIntersectionInViewportSpace, finalScale);
        RectI firstIntersectionBoundsInSurfaceSpace = (RectI)firstIntersectionInSurfaceSpace.AABBBounds.RoundOutwards();

        ShapeCorners viewportInSurfaceSpace = ViewportToSurface(viewportBoundsInViewportSpace, finalScale);
        RectD viewportBoundsInSurfaceSpace = viewportInSurfaceSpace.AABBBounds;
        RectD surfaceBoundsInSurfaceSpace = new(VecD.Zero, Surface.Size);
        RectI secondIntersectionInSurfaceSpace = (RectI)viewportBoundsInSurfaceSpace.Intersect(surfaceBoundsInSurfaceSpace).RoundOutwards();

        //Inflate makes sure rounding doesn't cut any pixels.
        RectI surfaceRectToRender = firstIntersectionBoundsInSurfaceSpace.Intersect(secondIntersectionInSurfaceSpace).Inflate(1);
        return surfaceRectToRender.Intersect(new RectI(VecI.Zero, Surface.Size)); // Clamp to surface size
    }

    private void DrawDebugRect(SKCanvas canvas, RectD rect)
    {
        canvas.DrawLine((float)rect.X, (float)rect.Y, (float)rect.Right, (float)rect.Y, _paint);
        canvas.DrawLine((float)rect.Right, (float)rect.Y, (float)rect.Right, (float)rect.Bottom, _paint);
        canvas.DrawLine((float)rect.Right, (float)rect.Bottom, (float)rect.X, (float)rect.Bottom, _paint);

        canvas.DrawLine((float)rect.X, (float)rect.Bottom, (float)rect.X, (float)rect.Y, _paint);
    }

    private ShapeCorners ViewportToSurface(RectI viewportRect, float scale)
    {
        return new ShapeCorners()
        {
            TopLeft = ViewportToSurface(viewportRect.TopLeft, scale),
            TopRight = ViewportToSurface(viewportRect.TopRight, scale),
            BottomLeft = ViewportToSurface(viewportRect.BottomLeft, scale),
            BottomRight = ViewportToSurface(viewportRect.BottomRight, scale),
        };
    }
    
    private ShapeCorners SurfaceToViewport(RectI viewportRect, float scale)
    {
        return new ShapeCorners()
        {
            TopLeft = SurfaceToViewport(viewportRect.TopLeft, scale),
            TopRight = SurfaceToViewport(viewportRect.TopRight, scale),
            BottomLeft = SurfaceToViewport(viewportRect.BottomLeft, scale),
            BottomRight = SurfaceToViewport(viewportRect.BottomRight, scale),
        };
    }

    private float CalculateFinalScale()
    {
        var scaleUniform = CalculateResolutionScale();
        float scale = (float)Scale * scaleUniform;
        return scale;
    }

    private float CalculateResolutionScale()
    {
        float scaleX = (float)Document.Width / Surface.Size.X;
        float scaleY = (float)Document.Height / Surface.Size.Y;
        var scaleUniform = Math.Min(scaleX, scaleY);
        return scaleUniform;
    }

    private VecD SurfaceToViewport(VecI surfacePoint, float scale)
    {
        VecD unscaledPoint = surfacePoint * scale;

        float angle = (float)Angle;
        if (FlipX)
        {
            unscaledPoint.X = -unscaledPoint.X;
            angle = 360 - angle;
        }
        if (FlipY)
        {
            unscaledPoint.Y = -unscaledPoint.Y;
            angle = 360 - angle;
        }

        VecD offseted = unscaledPoint + ContentPosition;

        float angleRadians = (float)(angle * Math.PI / 180);
        VecD rotated = offseted.Rotate(angleRadians, ContentPosition);

        return rotated;
    }

    private VecI ViewportToSurface(VecD viewportPoint, float scale)
    {
        float angle = (float)Angle;
        if (FlipX)
        {
            angle = 360 - angle;
        }
        if (FlipY)
        {
            angle = 360 - angle;
        }

        float angleRadians = (float)(angle * Math.PI / 180);
        VecD rotatedViewportPoint = (viewportPoint).Rotate(-angleRadians, ContentPosition);

        VecD unscaledPoint = rotatedViewportPoint - ContentPosition;

        if (FlipX)
            unscaledPoint.X = -unscaledPoint.X;
        if (FlipY)
            unscaledPoint.Y = -unscaledPoint.Y;

        VecI pos = new VecI(
            (int)Math.Round(unscaledPoint.X / scale),
            (int)Math.Round(unscaledPoint.Y / scale));

        return pos;
    }

    private static void BoundsChanged(Scene sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Rect bounds)
        {
            sender.CreateOutputSurface();
        }

        sender.RequestNextFrameRendering();
    }

    private static void RequestRendering(Scene sender, AvaloniaPropertyChangedEventArgs e)
    {
        sender.RequestNextFrameRendering();
    }
}
