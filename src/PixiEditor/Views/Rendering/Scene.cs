using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.DrawingApi.Skia.Extensions;
using PixiEditor.Extensions.UI.Overlays;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Converters;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Numerics;
using PixiEditor.ViewModels.Document;
using PixiEditor.Views.Overlays;
using PixiEditor.Views.Overlays.Pointers;
using PixiEditor.Views.Visuals;
using Bitmap = PixiEditor.DrawingApi.Core.Surfaces.Bitmap;
using Image = PixiEditor.DrawingApi.Core.Surfaces.ImageData.Image;
using Point = Avalonia.Point;

namespace PixiEditor.Views.Rendering;

internal class Scene : Zoombox.Zoombox, ICustomHitTest
{
    public static readonly StyledProperty<Texture> SurfaceProperty = AvaloniaProperty.Register<SurfaceControl, Texture>(
        nameof(Surface));

    public static readonly StyledProperty<DocumentViewModel> DocumentProperty =
        AvaloniaProperty.Register<Scene, DocumentViewModel>(
            nameof(Document));

    public static readonly StyledProperty<bool> FadeOutProperty = AvaloniaProperty.Register<Scene, bool>(
        nameof(FadeOut), false);

    public static readonly StyledProperty<ObservableCollection<Overlay>> AllOverlaysProperty =
        AvaloniaProperty.Register<Scene, ObservableCollection<Overlay>>(
            nameof(AllOverlays));

    public static readonly StyledProperty<string> CheckerImagePathProperty = AvaloniaProperty.Register<Scene, string>(
        nameof(CheckerImagePath));

    public static readonly StyledProperty<Cursor> DefaultCursorProperty = AvaloniaProperty.Register<Scene, Cursor>(
        nameof(DefaultCursor));

    public static readonly StyledProperty<ViewportColorChannels> ChannelsProperty =
        AvaloniaProperty.Register<Scene, ViewportColorChannels>(
            nameof(Channels));

    public Cursor DefaultCursor
    {
        get => GetValue(DefaultCursorProperty);
        set => SetValue(DefaultCursorProperty, value);
    }

    public string CheckerImagePath
    {
        get => GetValue(CheckerImagePathProperty);
        set => SetValue(CheckerImagePathProperty, value);
    }

    public ObservableCollection<Overlay> AllOverlays
    {
        get => GetValue(AllOverlaysProperty);
        set => SetValue(AllOverlaysProperty, value);
    }

    public bool FadeOut
    {
        get => GetValue(FadeOutProperty);
        set => SetValue(FadeOutProperty, value);
    }

    public DocumentViewModel Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public Texture Surface
    {
        get => GetValue(SurfaceProperty);
        set => SetValue(SurfaceProperty, value);
    }

    public ViewportColorChannels Channels
    {
        get => GetValue(ChannelsProperty);
        set => SetValue(ChannelsProperty, value);
    }

    private Bitmap? checkerBitmap;

    private Overlay? capturedOverlay;

    private List<Overlay> mouseOverOverlays = new();

    private double sceneOpacity = 1;

    static Scene()
    {
        AffectsRender<Scene>(BoundsProperty, WidthProperty, HeightProperty, ScaleProperty, AngleRadiansProperty,
            FlipXProperty,
            FlipYProperty, DocumentProperty, SurfaceProperty, AllOverlaysProperty);

        FadeOutProperty.Changed.AddClassHandler<Scene>(FadeOutChanged);
        SurfaceProperty.Changed.AddClassHandler<Scene>(SurfaceChanged);
        CheckerImagePathProperty.Changed.AddClassHandler<Scene>(CheckerImagePathChanged);
        AllOverlaysProperty.Changed.AddClassHandler<Scene>(ActiveOverlaysChanged);
        DefaultCursorProperty.Changed.AddClassHandler<Scene>(DefaultCursorChanged);
        ChannelsProperty.Changed.AddClassHandler<Scene>(ChannelsChanged);
    }

    private static void ChannelsChanged(Scene scene, AvaloniaPropertyChangedEventArgs args)
    {
        scene.InvalidateVisual();
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

        float angle = (float)MathUtil.RadiansToDegrees(AngleRadians);

        float resolutionScale = CalculateResolutionScale();

        RectD dirtyBounds = new RectD(0, 0, Document.Width / resolutionScale, Document.Height / resolutionScale);
        Rect dirtyRect = new Rect(0, 0, Document.Width / resolutionScale, Document.Height / resolutionScale);

        Surface.DrawingSurface.Flush();
        using var operation = new DrawSceneOperation(Surface, Document, CanvasPos, Scale * resolutionScale, angle,
            FlipX, FlipY,
            dirtyRect,
            Bounds,
            sceneOpacity,
            Channels.GetColorMatrix());

        var matrix = CalculateTransformMatrix();
        context.PushTransform(matrix);
        context.PushRenderOptions(new RenderOptions { BitmapInterpolationMode = BitmapInterpolationMode.None });

        var resolutionTransformation = context.PushTransform(Matrix.CreateScale(resolutionScale, resolutionScale));

        DrawCheckerboard(context, dirtyRect, operation.SurfaceRectToRender);

        resolutionTransformation.Dispose();

        Cursor = DefaultCursor;

        DrawOverlays(context, dirtyBounds, OverlayRenderSorting.Background);

        resolutionTransformation = context.PushTransform(Matrix.CreateScale(resolutionScale, resolutionScale));
        context.Custom(operation);

        resolutionTransformation.Dispose();
        DrawOverlays(context, dirtyBounds, OverlayRenderSorting.Foreground);
    }

    private void DrawOverlays(DrawingContext context, RectD dirtyBounds, OverlayRenderSorting sorting)
    {
        if (AllOverlays != null)
        {
            foreach (Overlay overlay in AllOverlays)
            {
                if (!overlay.IsVisible || overlay.OverlayRenderSorting != sorting)
                {
                    continue;
                }

                overlay.ZoomScale = Scale;

                if (!overlay.CanRender()) continue;

                overlay.RenderOverlay(context, dirtyBounds);
                Cursor = overlay.Cursor ?? DefaultCursor;
            }
        }
    }

    private void DrawCheckerboard(DrawingContext context, Rect dirtyBounds, RectI operationSurfaceRectToRender)
    {
        DrawCheckerBackgroundOperation checkerOperation = new DrawCheckerBackgroundOperation(
            dirtyBounds,
            (SKBitmap)checkerBitmap.Native,
            (float)Scale,
            operationSurfaceRectToRender.ToSkRect());
        context.Custom(checkerOperation);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (AllOverlays != null)
        {
            OverlayPointerArgs args = ConstructPointerArgs(e);
            foreach (Overlay overlay in AllOverlays)
            {
                if (!overlay.IsVisible || mouseOverOverlays.Contains(overlay) || !overlay.TestHit(args.Point)) continue;
                overlay.EnterPointer(args);
                mouseOverOverlays.Add(overlay);
            }

            e.Handled = args.Handled;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (AllOverlays != null)
        {
            OverlayPointerArgs args = ConstructPointerArgs(e);

            if (capturedOverlay != null)
            {
                capturedOverlay.MovePointer(args);
            }
            else
            {
                foreach (Overlay overlay in AllOverlays)
                {
                    if (!overlay.IsVisible) continue;

                    if (overlay.TestHit(args.Point))
                    {
                        if (!mouseOverOverlays.Contains(overlay))
                        {
                            overlay.EnterPointer(args);
                            mouseOverOverlays.Add(overlay);
                        }
                    }
                    else
                    {
                        if (mouseOverOverlays.Contains(overlay))
                        {
                            overlay.ExitPointer(args);
                            mouseOverOverlays.Remove(overlay);

                            e.Handled = args.Handled;
                            return;
                        }
                    }

                    overlay.MovePointer(args);
                }
            }

            e.Handled = args.Handled;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (AllOverlays != null)
        {
            OverlayPointerArgs args = ConstructPointerArgs(e);
            if (capturedOverlay != null)
            {
                capturedOverlay?.PressPointer(args);
            }
            else
            {
                for (var i = 0; i < mouseOverOverlays.Count; i++)
                {
                    var overlay = mouseOverOverlays[i];
                    if (args.Handled) break;
                    if (!overlay.IsVisible) continue;
                    overlay.PressPointer(args);
                }
            }

            e.Handled = args.Handled;
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (AllOverlays != null)
        {
            OverlayPointerArgs args = ConstructPointerArgs(e);
            for (var i = 0; i < mouseOverOverlays.Count; i++)
            {
                var overlay = mouseOverOverlays[i];
                if (args.Handled) break;
                if (!overlay.IsVisible) continue;

                overlay.ExitPointer(args);
                mouseOverOverlays.Remove(overlay);
                i--;
            }

            e.Handled = args.Handled;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerExited(e);
        if (AllOverlays != null)
        {
            OverlayPointerArgs args = ConstructPointerArgs(e);

            if (capturedOverlay != null)
            {
                capturedOverlay.ReleasePointer(args);
                capturedOverlay = null;
            }
            else
            {
                foreach (Overlay overlay in mouseOverOverlays)
                {
                    if (args.Handled) break;
                    if (!overlay.IsVisible) continue;

                    overlay.ReleasePointer(args);
                }
            }
        }
    }

    private OverlayPointerArgs ConstructPointerArgs(PointerEventArgs e)
    {
        return new OverlayPointerArgs
        {
            Point = ToCanvasSpace(e.GetPosition(this)),
            Modifiers = e.KeyModifiers,
            Pointer = new MouseOverlayPointer(e.Pointer, CaptureOverlay),
            PointerButton = e.GetMouseButton(this),
            InitialPressMouseButton = e is PointerReleasedEventArgs released
                ? released.InitialPressMouseButton
                : MouseButton.None,
        };
    }

    private VecD ToCanvasSpace(Point scenePosition)
    {
        Matrix transform = CalculateTransformMatrix();
        Point transformed = transform.Invert().Transform(scenePosition);
        return new VecD(transformed.X, transformed.Y);
    }

    private Matrix CalculateTransformMatrix()
    {
        Matrix transform = Matrix.Identity;
        transform = transform.Append(Matrix.CreateRotation((float)AngleRadians));
        transform = transform.Append(Matrix.CreateScale(FlipX ? -1 : 1, FlipY ? -1 : 1));
        transform = transform.Append(Matrix.CreateScale((float)Scale, (float)Scale));
        transform = transform.Append(Matrix.CreateTranslation(CanvasPos.X, CanvasPos.Y));
        return transform;
    }

    private float CalculateResolutionScale()
    {
        float scaleX = (float)Document.Width / Surface.Size.X;
        float scaleY = (float)Document.Height / Surface.Size.Y;
        var scaleUniform = Math.Min(scaleX, scaleY);
        return scaleUniform;
    }

    private void CaptureOverlay(Overlay? overlay, IPointer pointer)
    {
        if (AllOverlays == null) return;
        if (overlay == null)
        {
            pointer.Capture(null);
            mouseOverOverlays.Clear();
            capturedOverlay = null;
            return;
        }

        if (!AllOverlays.Contains(overlay)) return;

        pointer.Capture(this);
        capturedOverlay = overlay;
        mouseOverOverlays.Clear();
        mouseOverOverlays.Add(overlay);
    }

    private void OverlayCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
        if (e.OldItems != null)
        {
            foreach (Overlay overlay in e.OldItems)
            {
                overlay.RefreshRequested -= QueueRender;
            }
        }

        if (e.NewItems != null)
        {
            foreach (Overlay overlay in e.NewItems)
            {
                overlay.RefreshRequested += QueueRender;
            }
        }
    }

    private void QueueRender()
    {
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Render);
    }

    private static void FadeOutChanged(Scene scene, AvaloniaPropertyChangedEventArgs e)
    {
        scene.sceneOpacity = e.NewValue is true ? 0 : 1;
        scene.InvalidateVisual();
    }

    private static void ActiveOverlaysChanged(Scene scene, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is ObservableCollection<Overlay> oldOverlays)
        {
            oldOverlays.CollectionChanged -= scene.OverlayCollectionChanged;
        }

        if (e.NewValue is ObservableCollection<Overlay> newOverlays)
        {
            newOverlays.CollectionChanged += scene.OverlayCollectionChanged;
        }
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

    private static void SurfaceChanged(Scene scene, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Texture surface)
        {
            scene.ContentDimensions = surface.Size;
        }
    }

    private static void DefaultCursorChanged(Scene scene, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Cursor cursor)
        {
            scene.Cursor = cursor;
        }
    }

    bool ICustomHitTest.HitTest(Point point)
    {
        return Bounds.Contains(point);
    }
}

internal class DrawSceneOperation : SkiaDrawOperation
{
    public Texture Surface { get; set; }
    public DocumentViewModel Document { get; set; }
    public VecD ContentPosition { get; set; }
    public double Scale { get; set; }
    public double Angle { get; set; }
    public bool FlipX { get; set; }
    public bool FlipY { get; set; }
    public Rect ViewportBounds { get; }
    public ColorMatrix ColorMatrix { get; }

    public RectI SurfaceRectToRender { get; }

    private SKPaint _paint = new SKPaint();

    private bool hardwareAccelerationAvailable = DrawingBackendApi.Current.IsHardwareAccelerated;

    public DrawSceneOperation(Texture surface, DocumentViewModel document, VecD contentPosition, double scale,
        double angle, bool flipX, bool flipY, Rect dirtyBounds, Rect viewportBounds, double opacity,
        ColorMatrix colorMatrix) : base(dirtyBounds)
    {
        Surface = surface;
        Document = document;
        ContentPosition = contentPosition;
        Scale = scale;
        Angle = angle;
        FlipX = flipX;
        FlipY = flipY;
        ColorMatrix = colorMatrix;
        ViewportBounds = viewportBounds;
        _paint.Color = _paint.Color.WithAlpha((byte)(opacity * 255));
        SurfaceRectToRender = FindRectToRender((float)scale);
    }

    public override void Render(ISkiaSharpApiLease lease)
    {
        if (Surface == null || Surface.IsDisposed || Document == null) return;

        SKCanvas canvas = lease.SkCanvas;

        canvas.Save();

        if (SurfaceRectToRender.IsZeroOrNegativeArea)
        {
            canvas.Restore();
            return;
        }

        using var ctx = DrawingBackendApi.Current.RenderOnDifferentGrContext(lease.GrContext);


        var matrixValues = new float[ColorMatrix.Width * ColorMatrix.Height];
        ColorMatrix.TryGetMembers(matrixValues);

        _paint.ColorFilter = SKColorFilter.CreateColorMatrix(matrixValues);

        if (!hardwareAccelerationAvailable)
        {
            // snapshotting wanted region on CPU is faster than rendering whole surface on CPU,
            // but slower than rendering whole surface on GPU
            using Image snapshot = Surface.DrawingSurface.Snapshot(SurfaceRectToRender);
            canvas.DrawImage((SKImage)snapshot.Native, SurfaceRectToRender.X, SurfaceRectToRender.Y, _paint);
        }
        else
        {
            canvas.DrawSurface(Surface.DrawingSurface.Native as SKSurface, 0, 0, _paint);
        }

        canvas.Restore();
    }

    private RectI FindRectToRender(float finalScale)
    {
        ShapeCorners surfaceInViewportSpace = SurfaceToViewport(new RectI(VecI.Zero, Surface.Size), finalScale);
        RectI surfaceBoundsInViewportSpace = (RectI)surfaceInViewportSpace.AABBBounds.RoundOutwards();
        RectI viewportBoundsInViewportSpace =
            (RectI)(new RectD(ViewportBounds.X, ViewportBounds.Y, ViewportBounds.Width, ViewportBounds.Height))
            .RoundOutwards();
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
