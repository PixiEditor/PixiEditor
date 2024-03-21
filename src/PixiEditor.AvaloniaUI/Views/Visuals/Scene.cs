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

    public static readonly StyledProperty<VecD> DimensionsProperty = AvaloniaProperty.Register<Scene, VecD>(
        nameof(Dimensions));

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
    
    public VecD Dimensions
    {
        get => GetValue(DimensionsProperty);
        set => SetValue(DimensionsProperty, value);
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

    private SKSurface _outputSurface;
    private SKPaint _paint = new SKPaint();
    private GRContext? gr;

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
        
        //FinalBounds = new(dime)
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (Surface == null || Document == null) return;

        SKCanvas canvas = _outputSurface.Canvas;

        canvas.Save();
        canvas.ClipRect(new SKRect(0, 0, (float)Bounds.Width, (float)Bounds.Height));
        canvas.Clear(SKColors.Transparent);

        float resolutionScale = CalculateResolutionScale();
        float finalScale = CalculateFinalScale();
        float radians = (float)(Angle * Math.PI / 180);

        RectD viewport = new(FinalBounds.X, FinalBounds.Y, FinalBounds.Width, FinalBounds.Height);

        ShapeCorners surfaceInViewportSpace = SurfaceToViewport(new RectI(VecI.Zero, Surface.Size), finalScale, radians);
        RectI surfaceBoundsInViewportSpace = (RectI)surfaceInViewportSpace.AABBBounds.RoundOutwards();
        RectI viewportBoundsInViewportSpace = (RectI)(new RectD(FinalBounds.X, FinalBounds.Y, FinalBounds.Width, FinalBounds.Height)).RoundOutwards();
        RectI firstIntersectionInViewportSpace = surfaceBoundsInViewportSpace.Intersect(viewportBoundsInViewportSpace);
        ShapeCorners firstIntersectionInSurfaceSpace = ViewportToSurface(firstIntersectionInViewportSpace, finalScale, radians);
        RectI firstIntersectionBoundsInSurfaceSpace = (RectI)firstIntersectionInSurfaceSpace.AABBBounds.RoundOutwards();

        VecD scaledCenter = ViewportToSurface(new VecD(FinalBounds.Center.X, FinalBounds.Center.Y), finalScale, radians);
        ShapeCorners viewportInSurfaceSpace = new ShapeCorners(scaledCenter, Dimensions * resolutionScale).AsRotated(-radians, scaledCenter);
        RectD viewportBoundsInSurfaceSpace = viewportInSurfaceSpace.AABBBounds;
        RectD surfaceBoundsInSurfaceSpace = new(VecD.Zero, Surface.Size);
        RectI secondIntersectionInSurfaceSpace = (RectI)viewportBoundsInSurfaceSpace.Intersect(surfaceBoundsInSurfaceSpace).RoundOutwards();

        //Inflate makes sure rounding doesn't cut any pixels.
        RectI surfaceRectToRender = firstIntersectionBoundsInSurfaceSpace.Intersect(secondIntersectionInSurfaceSpace).Inflate(1);
        surfaceRectToRender = surfaceRectToRender.Intersect(new RectI(VecI.Zero, Surface.Size)); // Clamp to surface size

        if (surfaceRectToRender.IsZeroOrNegativeArea)
        {
            canvas.Restore();
            canvas.Flush();
            RequestNextFrameRendering();
            return;
        }

        canvas.RotateDegrees((float)Angle, ContentPosition.X, ContentPosition.Y);
        canvas.Scale(finalScale, finalScale, ContentPosition.X, ContentPosition.Y);
        canvas.Translate(ContentPosition.X, ContentPosition.Y);

        using Image snapshot = Surface.DrawingSurface.Snapshot((RectI)surfaceRectToRender);
        canvas.DrawImage((SKImage)snapshot.Native, (float)surfaceRectToRender.X, (float)surfaceRectToRender.Y, _paint);

        canvas.Restore();

        canvas.Flush();
        RequestNextFrameRendering();
    }

    private void DrawDebugRect(SKCanvas canvas, RectD rect)
    {
        canvas.DrawLine((float)rect.X, (float)rect.Y, (float)rect.Right, (float)rect.Y, _paint);
        canvas.DrawLine((float)rect.Right, (float)rect.Y, (float)rect.Right, (float)rect.Bottom, _paint);
        canvas.DrawLine((float)rect.Right, (float)rect.Bottom, (float)rect.X, (float)rect.Bottom, _paint);

        canvas.DrawLine((float)rect.X, (float)rect.Bottom, (float)rect.X, (float)rect.Y, _paint);
    }

    private ShapeCorners ViewportToSurface(RectI viewportRect, float scale, float angleRadians)
    {
        return new ShapeCorners()
        {
            TopLeft = ViewportToSurface(viewportRect.TopLeft, scale, angleRadians),
            TopRight = ViewportToSurface(viewportRect.TopRight, scale, angleRadians),
            BottomLeft = ViewportToSurface(viewportRect.BottomLeft, scale, angleRadians),
            BottomRight = ViewportToSurface(viewportRect.BottomRight, scale, angleRadians),
        };
    }
    
    private ShapeCorners SurfaceToViewport(RectI viewportRect, float scale, float angleRadians)
    {
        return new ShapeCorners()
        {
            TopLeft = SurfaceToViewport(viewportRect.TopLeft, scale, angleRadians),
            TopRight = SurfaceToViewport(viewportRect.TopRight, scale, angleRadians),
            BottomLeft = SurfaceToViewport(viewportRect.BottomLeft, scale, angleRadians),
            BottomRight = SurfaceToViewport(viewportRect.BottomRight, scale, angleRadians),
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

    private VecD SurfaceToViewport(VecI surfacePoint, float scale, float angleRadians)
    {
        VecD unscaledPoint = surfacePoint * scale;
        VecD offseted = unscaledPoint + ContentPosition;

        return offseted.Rotate(angleRadians, ContentPosition);
    }

    private VecI ViewportToSurface(VecD viewportPoint, float scale, float angleRadians)
    {
        VecD rotatedViewportPoint = (viewportPoint).Rotate(-angleRadians, ContentPosition);
        VecD unscaledPoint = rotatedViewportPoint - ContentPosition;
        return new VecI(
            (int)Math.Round(unscaledPoint.X / scale),
            (int)Math.Round(unscaledPoint.Y / scale));
    }

    private static void BoundsChanged(Scene sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Rect bounds)
        {
            sender.CreateOutputSurface();
        }
    }
}
