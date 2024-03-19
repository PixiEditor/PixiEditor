using System.Collections.Generic;
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

        canvas.Save();
        canvas.ClipRect(new SKRect(0, 0, (float)Bounds.Width, (float)Bounds.Height));
        canvas.Clear(SKColors.Transparent);

        var scale = CalculateFinalScale();

        /*canvas.RotateDegrees((float)Angle, ContentPosition.X, ContentPosition.Y);
        canvas.Scale(scale, scale, ContentPosition.X, ContentPosition.Y);

        canvas.Translate(ContentPosition.X, ContentPosition.Y);*/

        /*VecI surfaceStart = ViewportToSurface(new VecI(0, 0), scale);
        VecI surfaceEnd = ViewportToSurface(new VecI((int)Bounds.Width, (int)Bounds.Height), scale);*/

        float radians = (float)(Angle * Math.PI / 180);
        VecD topLeft = SurfaceToViewport(new VecI(0, 0), scale, radians);
        VecD bottomRight = SurfaceToViewport(Surface.Size, scale, radians);
        VecD topRight = SurfaceToViewport(new VecI(Surface.Size.X, 0), scale, radians);
        VecD bottomLeft = SurfaceToViewport(new VecI(0, Surface.Size.Y), scale, radians);

        VecD[] surfaceBounds = new VecD[] { topLeft, topRight, bottomRight, bottomLeft };
        List<VecD> intersections = FindIntersectionPoints(surfaceBounds, new VecD(Bounds.Width, Bounds.Height));

        // debug circles
        _paint.Color = SKColors.Red;
        foreach (var intersection in intersections)
        {
            canvas.DrawCircle((float)intersection.X, (float)intersection.Y, 5, _paint);
        }

        /*if (IsOutOfBounds(surfaceStart, surfaceEnd))
        {
            canvas.Restore();
            canvas.Flush();
            RequestNextFrameRendering();
            return;
        }

        int x = Math.Max(0, surfaceStart.X);
        int y = Math.Max(0, surfaceStart.Y);
        int width = Math.Min(Surface.Size.X, surfaceEnd.X - x) + 1;
        int height = Math.Min(Surface.Size.Y, surfaceEnd.Y - y) + 1;

        visibleSurfaceRect = new RectI(x, y, width, height);
        //visibleSurfaceRect = RotateRect(visibleSurfaceRect, (float)Angle, ContentPosition);

        using Image snapshot = Surface.DrawingSurface.Snapshot(visibleSurfaceRect);
        canvas.DrawImage((SKImage)snapshot.Native, x, y, _paint);*/

        canvas.Restore();

        canvas.Flush();
        RequestNextFrameRendering();
    }

    private RectD RectFromIntersections(List<VecD> intersections)
    {
        if (intersections.Count < 4)
        {
            return new RectD(0, 0, 0, 0);
        }

        double minX = double.MaxValue;
        double minY = double.MaxValue;
        double maxX = double.MinValue;
        double maxY = double.MinValue;

        foreach (var intersection in intersections)
        {
            if (intersection.X < minX)
            {
                minX = intersection.X;
            }
            if (intersection.X > maxX)
            {
                maxX = intersection.X;
            }
            if (intersection.Y < minY)
            {
                minY = intersection.Y;
            }
            if (intersection.Y > maxY)
            {
                maxY = intersection.Y;
            }
        }

        return new RectD(minX, minY, maxX - minX, maxY - minY);
    }

    private static List<VecD> FindIntersectionPoints(VecD[] canvasPoints, VecD viewportSize)
    {
        List<VecD> intersections = new List<VecD>();
        VecD[] viewportBounds = new VecD[]
        {
            new VecD(0, 0),
            new VecD(viewportSize.X, 0),
            new VecD(viewportSize.X, viewportSize.Y),
            new VecD(0, viewportSize.Y)
        };

        for (int i = 0; i < canvasPoints.Length; i++)
        {
            VecD p1 = canvasPoints[i];
            VecD p2 = canvasPoints[(i + 1) % canvasPoints.Length];

            for (int j = 0; j < viewportBounds.Length; j++)
            {
                VecD p3 = viewportBounds[j];
                VecD p4 = viewportBounds[(j + 1) % viewportBounds.Length];

                VecD? intersection = FindIntersection(p1, p2, p3, p4);
                if (intersection != null)
                {
                    intersections.Add(intersection.Value);
                }
                else if (p1.X >= 0 && p1.X <= viewportSize.X && p1.Y >= 0 && p1.Y <= viewportSize.Y)
                {
                    intersections.Add(p1);
                }
                else if (p2.X >= 0 && p2.X <= viewportSize.X && p2.Y >= 0 && p2.Y <= viewportSize.Y)
                {
                    intersections.Add(p2);
                }
            }
        }

        return intersections;
    }

    private static VecD? FindIntersection(VecD p1, VecD p2, VecD p3, VecD p4)
    {
        double ua = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X)) /
                    ((p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y));

        double ub = ((p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X)) /
                    ((p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y));

        if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
        {
            double x = p1.X + ua * (p2.X - p1.X);
            double y = p1.Y + ua * (p2.Y - p1.Y);
            return new VecD(x, y);
        }

        return null;
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

    /*private VecI ViewportToSurface(VecI surfacePoint, float scale)
    {
        float radians = (float)(Angle * Math.PI / 180);
        VecD rotatedSurfacePoint = ((VecD)surfacePoint).Rotate(radians, ContentPosition / scale);
        return new VecI(
            (int)((rotatedSurfacePoint.X - ContentPosition.X) / scale),
            (int)((rotatedSurfacePoint.Y - ContentPosition.Y) / scale));
    }*/

    private VecD SurfaceToViewport(VecI surfacePoint, float scale, float angleRadians)
    {
        VecD unscaledPoint = surfacePoint * scale;
        VecD offseted = unscaledPoint + ContentPosition;

        return offseted.Rotate(angleRadians, ContentPosition);
    }

    private VecI ViewportToSurface(VecI viewportPoint, float scale, float angleRadians)
    {
        VecD rotatedViewportPoint = ((VecD)viewportPoint).Rotate(-angleRadians, ContentPosition);
        VecD unscaledPoint = rotatedViewportPoint - ContentPosition;
        return new VecI(
            (int)(unscaledPoint.X / scale),
            (int)(unscaledPoint.Y / scale));
    }

    private static void BoundsChanged(Scene sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Rect bounds)
        {
            sender.CreateOutputSurface();
        }
    }
}
