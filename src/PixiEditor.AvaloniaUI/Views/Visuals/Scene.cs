using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Helpers.Converters;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.AvaloniaUI.Views.Overlays;
using PixiEditor.AvaloniaUI.Views.Overlays.TransformOverlay;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Skia;
using Image = PixiEditor.DrawingApi.Core.Surface.ImageData.Image;

namespace PixiEditor.AvaloniaUI.Views.Visuals;

internal class Scene : Control
{
    public static readonly StyledProperty<Surface> SurfaceProperty = AvaloniaProperty.Register<SurfaceControl, Surface>(
        nameof(Surface));

    public static readonly StyledProperty<double> ScaleProperty = AvaloniaProperty.Register<Scene, double>(
        nameof(Scale), 1);

    public static readonly StyledProperty<VecI> ContentPositionProperty = AvaloniaProperty.Register<Scene, VecI>(
        nameof(ContentPosition));

    public static readonly StyledProperty<DocumentViewModel> DocumentProperty =
        AvaloniaProperty.Register<Scene, DocumentViewModel>(
            nameof(Document));

    public static readonly StyledProperty<double> AngleProperty = AvaloniaProperty.Register<Scene, double>(
        nameof(Angle), 0);

    public static readonly StyledProperty<bool> FlipXProperty = AvaloniaProperty.Register<Scene, bool>(
        nameof(FlipX), false);

    public static readonly StyledProperty<bool> FlipYProperty = AvaloniaProperty.Register<Scene, bool>(
        nameof(FlipY), false);

    public static readonly StyledProperty<bool> FadeOutProperty = AvaloniaProperty.Register<Scene, bool>(
        nameof(FadeOut), false);

    public static readonly StyledProperty<ObservableCollection<Overlay>> ActiveOverlaysProperty = AvaloniaProperty.Register<Scene, ObservableCollection<Overlay>>(
        nameof(ActiveOverlays));

    public static readonly StyledProperty<string> CheckerImagePathProperty = AvaloniaProperty.Register<Scene, string>(
        nameof(CheckerImagePath));

    public string CheckerImagePath
    {
        get => GetValue(CheckerImagePathProperty);
        set => SetValue(CheckerImagePathProperty, value);
    }

    public ObservableCollection<Overlay> ActiveOverlays
    {
        get => GetValue(ActiveOverlaysProperty);
        set => SetValue(ActiveOverlaysProperty, value);
    }

    public bool FadeOut
    {
        get => GetValue(FadeOutProperty);
        set => SetValue(FadeOutProperty, value);
    }

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

    private Bitmap? checkerBitmap;

    static Scene()
    {
        AffectsRender<Scene>(BoundsProperty, WidthProperty, HeightProperty, ScaleProperty, AngleProperty, FlipXProperty,
            FlipYProperty, ContentPositionProperty, DocumentProperty, SurfaceProperty);
        BoundsProperty.Changed.AddClassHandler<Scene>(BoundsChanged);
        FlipXProperty.Changed.AddClassHandler<Scene>(RequestRendering);
        FlipYProperty.Changed.AddClassHandler<Scene>(RequestRendering);
        FadeOutProperty.Changed.AddClassHandler<Scene>(FadeOutChanged);
        CheckerImagePathProperty.Changed.AddClassHandler<Scene>(CheckerImagePathChanged);
        ActiveOverlaysProperty.Changed.AddClassHandler<Scene>(ActiveOverlaysChanged);
    }

    public Scene()
    {
        ClipToBounds = true;
        Transitions = new Transitions
        {
            new DoubleTransition { Property = OpacityProperty, Duration = new TimeSpan(0, 0, 0, 0, 100) }
        };
    }

    public override void Render(DrawingContext context)
    {
        if (Surface == null || Document == null) return;

        float finalScale = CalculateFinalScale();
        context.PushTransform(Matrix.CreateTranslation(ContentPosition.X, ContentPosition.Y));
        context.PushTransform(Matrix.CreateScale(finalScale, finalScale));

        float angle = (float)Angle;
        if (FlipX)
        {
            angle = 360 - angle;
        }

        if (FlipY)
        {
            angle = 360 - angle;
        }

        context.PushTransform(Matrix.CreateRotation(MathUtil.AngleToRadians(angle)));
        context.PushTransform(Matrix.CreateScale(FlipX ? -1 : 1, FlipY ? -1 : 1));

        var operation = new DrawSceneOperation(Surface, Document, ContentPosition, finalScale, Angle, FlipX, FlipY, Bounds,
            Opacity, (SKBitmap)checkerBitmap.Native);
        context.Custom(operation);

        if (ActiveOverlays != null)
        {
            foreach (Overlay overlay in ActiveOverlays)
            {
                overlay.ZoomScale = finalScale;
                if(!overlay.IsVisible) continue;

                overlay.Render(context);
            }
        }
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

    private static void BoundsChanged(Scene sender, AvaloniaPropertyChangedEventArgs e)
    {
        sender.InvalidateVisual();
    }

    private static void RequestRendering(Scene sender, AvaloniaPropertyChangedEventArgs e)
    {
        sender.InvalidateVisual();
    }

    private static void FadeOutChanged(Scene scene, AvaloniaPropertyChangedEventArgs e)
    {
        scene.Opacity = e.NewValue is true ? 0 : 1;
    }

    private static void ActiveOverlaysChanged(Scene scene, AvaloniaPropertyChangedEventArgs e)
    {
        scene.InvalidateVisual();
        if (e.OldValue is ObservableCollection<Overlay> oldOverlays)
        {
            oldOverlays.CollectionChanged -= scene.OverlayCollectionChanged;
        }
        if (e.NewValue is ObservableCollection<Overlay> newOverlays)
        {
            newOverlays.CollectionChanged += scene.OverlayCollectionChanged;
        }
    }

    private void OverlayCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private static void CheckerImagePathChanged(Scene scene, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is string path)
        {
            scene.checkerBitmap = ImagePathToBitmapConverter.LoadDrawingApiBitmapFromRelativePath(path);
        }
        else
        {
            scene.checkerBitmap = null;
        }
    }
}

internal class DrawSceneOperation : SkiaDrawOperation
{
    public Surface Surface { get; set; }
    public DocumentViewModel Document { get; set; }
    public VecI ContentPosition { get; set; }
    public double Scale { get; set; }
    public double Angle { get; set; }
    public bool FlipX { get; set; }
    public bool FlipY { get; set; }

    public SKBitmap? CheckerBitmap { get; set; }

    private SKPaint _paint = new SKPaint();
    private SKPaint _checkerPaint;

    public DrawSceneOperation(Surface surface, DocumentViewModel document, VecI contentPosition, double scale,
        double angle, bool flipX, bool flipY, Rect bounds, double opacity, SKBitmap checkerBitmap) : base(bounds)
    {
        Surface = surface;
        Document = document;
        ContentPosition = contentPosition;
        Scale = scale;
        Angle = angle;
        FlipX = flipX;
        FlipY = flipY;
        CheckerBitmap = checkerBitmap;
        _paint.Color = _paint.Color.WithAlpha((byte)(opacity * 255));

        float checkerScale = (float)ZoomToViewportConverter.ZoomToViewport(16, scale) * 0.25f;
        _checkerPaint = new SKPaint()
        {
            Shader = SKShader.CreateBitmap(
                CheckerBitmap,
                SKShaderTileMode.Repeat, SKShaderTileMode.Repeat,
                SKMatrix.CreateScale(checkerScale, checkerScale)),
            FilterQuality = SKFilterQuality.None
        };
    }

    public override void Render(ISkiaSharpApiLease lease)
    {
        if (Surface == null || Document == null) return;

        SKCanvas canvas = lease.SkCanvas;

        canvas.Save();

        RectI surfaceRectToRender = FindRectToRender((float)Scale);

        if (surfaceRectToRender.IsZeroOrNegativeArea)
        {
            canvas.Restore();
            canvas.Flush();
            return;
        }

        float angle = (float)Angle;
        if (FlipX)
        {
            angle = 360 - angle;
        }

        if (FlipY)
        {
            angle = 360 - angle;
        }

        DrawCheckerboard(canvas, surfaceRectToRender);

        using Image snapshot = Surface.DrawingSurface.Snapshot(surfaceRectToRender);
        canvas.DrawImage((SKImage)snapshot.Native, surfaceRectToRender.X, surfaceRectToRender.Y, _paint);

        canvas.Restore();

        canvas.Flush();
    }

    private void DrawCheckerboard(SKCanvas canvas, RectI surfaceRectToRender)
    {
        if (CheckerBitmap != null)
        {
            canvas.DrawRect(surfaceRectToRender.ToSkRect(), _checkerPaint);
        }
    }

    private RectI FindRectToRender(float finalScale)
    {
        ShapeCorners surfaceInViewportSpace = SurfaceToViewport(new RectI(VecI.Zero, Surface.Size), finalScale);
        RectI surfaceBoundsInViewportSpace = (RectI)surfaceInViewportSpace.AABBBounds.RoundOutwards();
        RectI viewportBoundsInViewportSpace =
            (RectI)(new RectD(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height)).RoundOutwards();
        RectI firstIntersectionInViewportSpace = surfaceBoundsInViewportSpace.Intersect(viewportBoundsInViewportSpace);
        ShapeCorners firstIntersectionInSurfaceSpace = ViewportToSurface(firstIntersectionInViewportSpace, finalScale);
        RectI firstIntersectionBoundsInSurfaceSpace = (RectI)firstIntersectionInSurfaceSpace.AABBBounds.RoundOutwards();

        ShapeCorners viewportInSurfaceSpace = ViewportToSurface(viewportBoundsInViewportSpace, finalScale);
        RectD viewportBoundsInSurfaceSpace = viewportInSurfaceSpace.AABBBounds;
        RectD surfaceBoundsInSurfaceSpace = new(VecD.Zero, Surface.Size);
        RectI secondIntersectionInSurfaceSpace =
            (RectI)viewportBoundsInSurfaceSpace.Intersect(surfaceBoundsInSurfaceSpace).RoundOutwards();

        //Inflate makes sure rounding doesn't cut any pixels.
        RectI surfaceRectToRender =
            firstIntersectionBoundsInSurfaceSpace.Intersect(secondIntersectionInSurfaceSpace).Inflate(1);
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

    public override bool Equals(ICustomDrawOperation? other)
    {
        return false;
    }
}
