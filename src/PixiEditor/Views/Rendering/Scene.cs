using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Interop.Avalonia.Core;
using PixiEditor.Extensions.UI.Overlays;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Converters;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Rendering;
using Drawie.Numerics;
using Drawie.Skia;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Workspace;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Nodes.Workspace;
using PixiEditor.Views.Overlays;
using PixiEditor.Views.Overlays.Pointers;
using PixiEditor.Views.Visuals;
using Bitmap = Drawie.Backend.Core.Surfaces.Bitmap;
using Color = Drawie.Backend.Core.ColorsImpl.Color;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;
using Point = Avalonia.Point;
using TileMode = Drawie.Backend.Core.Surfaces.TileMode;

namespace PixiEditor.Views.Rendering;

internal class Scene : Zoombox.Zoombox, ICustomHitTest
{
    public static readonly StyledProperty<DocumentViewModel> DocumentProperty =
        AvaloniaProperty.Register<Scene, DocumentViewModel>(
            nameof(Document));

    public static readonly StyledProperty<bool> FadeOutProperty = AvaloniaProperty.Register<Scene, bool>(
        nameof(FadeOut), false);

    public static readonly StyledProperty<ObservableCollection<Overlay>> AllOverlaysProperty =
        AvaloniaProperty.Register<Scene, ObservableCollection<Overlay>>(
            nameof(AllOverlays));

    public static readonly StyledProperty<Cursor> DefaultCursorProperty = AvaloniaProperty.Register<Scene, Cursor>(
        nameof(DefaultCursor));

    public static readonly StyledProperty<ViewportColorChannels> ChannelsProperty =
        AvaloniaProperty.Register<Scene, ViewportColorChannels>(
            nameof(Channels));

    public static readonly StyledProperty<SceneRenderer> SceneRendererProperty =
        AvaloniaProperty.Register<Scene, SceneRenderer>(
            nameof(SceneRenderer));

    public static readonly StyledProperty<bool> AutoBackgroundScaleProperty = AvaloniaProperty.Register<Scene, bool>(
        nameof(AutoBackgroundScale), true);

    public bool AutoBackgroundScale
    {
        get => GetValue(AutoBackgroundScaleProperty);
        set => SetValue(AutoBackgroundScaleProperty, value);
    }

    public static readonly StyledProperty<double> CustomBackgroundScaleXProperty =
        AvaloniaProperty.Register<Scene, double>(
            nameof(CustomBackgroundScaleX));

    public double CustomBackgroundScaleX
    {
        get => GetValue(CustomBackgroundScaleXProperty);
        set => SetValue(CustomBackgroundScaleXProperty, value);
    }

    public static readonly StyledProperty<double> CustomBackgroundScaleYProperty =
        AvaloniaProperty.Register<Scene, double>(
            nameof(CustomBackgroundScaleY));

    public double CustomBackgroundScaleY
    {
        get => GetValue(CustomBackgroundScaleYProperty);
        set => SetValue(CustomBackgroundScaleYProperty, value);
    }

    public static readonly StyledProperty<Bitmap> BackgroundBitmapProperty = AvaloniaProperty.Register<Scene, Bitmap>(
        nameof(BackgroundBitmap));

    public Bitmap BackgroundBitmap
    {
        get => GetValue(BackgroundBitmapProperty);
        set => SetValue(BackgroundBitmapProperty, value);
    }

    public SceneRenderer SceneRenderer
    {
        get => GetValue(SceneRendererProperty);
        set => SetValue(SceneRendererProperty, value);
    }

    public Cursor DefaultCursor
    {
        get => GetValue(DefaultCursorProperty);
        set => SetValue(DefaultCursorProperty, value);
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

    public ViewportColorChannels Channels
    {
        get => GetValue(ChannelsProperty);
        set => SetValue(ChannelsProperty, value);
    }

    public string RenderOutput
    {
        get { return (string)GetValue(RenderOutputProperty); }
        set { SetValue(RenderOutputProperty, value); }
    }

    public static readonly StyledProperty<int> MaxBilinearSamplingSizeProperty = AvaloniaProperty.Register<Scene, int>(
        nameof(MaxBilinearSamplingSize), 4096);

    public int MaxBilinearSamplingSize
    {
        get => GetValue(MaxBilinearSamplingSizeProperty);
        set => SetValue(MaxBilinearSamplingSizeProperty, value);
    }

    public static readonly StyledProperty<Guid> ViewportIdProperty = AvaloniaProperty.Register<Scene, Guid>(
        nameof(ViewportId));

    public Guid ViewportId
    {
        get => GetValue(ViewportIdProperty);
        set => SetValue(ViewportIdProperty, value);
    }

    private Overlay? capturedOverlay;

    private List<Overlay> mouseOverOverlays = new();

    private double sceneOpacity = 1;

    private Paint checkerPaint;

    private CompositionSurfaceVisual surfaceVisual;
    private Compositor compositor;

    private readonly Action update;
    private bool updateQueued;

    private CompositionDrawingSurface? surface;

    private string info = string.Empty;
    private bool initialized = false;
    private RenderApiResources resources;

    private DrawingSurface framebuffer;
    private Texture renderTexture;

    private PixelSize lastSize = PixelSize.Empty;
    private Cursor lastCursor;
    private VecD lastMousePosition;

    public static readonly StyledProperty<string> RenderOutputProperty =
        AvaloniaProperty.Register<Scene, string>("RenderOutput");

    static Scene()
    {
        AffectsRender<Scene>(BoundsProperty, WidthProperty, HeightProperty, ScaleProperty, AngleRadiansProperty,
            FlipXProperty,
            FlipYProperty, DocumentProperty, AllOverlaysProperty, ContentDimensionsProperty,
            AutoBackgroundScaleProperty, CustomBackgroundScaleXProperty, CustomBackgroundScaleYProperty);

        FadeOutProperty.Changed.AddClassHandler<Scene>(FadeOutChanged);
        AllOverlaysProperty.Changed.AddClassHandler<Scene>(ActiveOverlaysChanged);
        DefaultCursorProperty.Changed.AddClassHandler<Scene>(DefaultCursorChanged);
        ChannelsProperty.Changed.AddClassHandler<Scene>(Refresh);
        DocumentProperty.Changed.AddClassHandler<Scene>(DocumentChanged);
        FlipXProperty.Changed.AddClassHandler<Scene>(Refresh);
        FlipYProperty.Changed.AddClassHandler<Scene>(Refresh);
        AutoBackgroundScaleProperty.Changed.AddClassHandler<Scene>(Refresh);
        CustomBackgroundScaleXProperty.Changed.AddClassHandler<Scene>(Refresh);
        CustomBackgroundScaleYProperty.Changed.AddClassHandler<Scene>(Refresh);
        BackgroundBitmapProperty.Changed.AddClassHandler<Scene>(Refresh);
        RenderOutputProperty.Changed.AddClassHandler<Scene>(Refresh);
        RenderOutputProperty.Changed.AddClassHandler<Scene>(UpdateRenderOutput);
    }

    private static void Refresh(Scene scene, AvaloniaPropertyChangedEventArgs args)
    {
        scene.QueueNextFrame();
    }

    public Scene()
    {
        ClipToBounds = true;
        Transitions = new Transitions
        {
            new DoubleTransition { Property = OpacityProperty, Duration = new TimeSpan(0, 0, 0, 0, 100) }
        };

        update = UpdateFrame;
        QueueNextFrame();
    }

    private ChunkResolution CalculateResolution()
    {
        VecD densityVec = Dimensions.Divide(RealDimensions);
        double density = Math.Min(densityVec.X, densityVec.Y);
        return density switch
        {
            > 8.01 => ChunkResolution.Eighth,
            > 4.01 => ChunkResolution.Quarter,
            > 2.01 => ChunkResolution.Half,
            _ => ChunkResolution.Full
        };
    }

    internal SamplingOptions CalculateSampling()
    {
        if (Document == null)
            return SamplingOptions.Default;

        if (Document.SizeBindable.LongestAxis > MaxBilinearSamplingSize)
        {
            return SamplingOptions.Default;
        }

        VecD densityVec = Dimensions.Divide(RealDimensions);
        double density = Math.Min(densityVec.X, densityVec.Y);
        return density > 1
            ? SamplingOptions.Bilinear
            : SamplingOptions.Default;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        InitializeComposition();
    }

    protected override async void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        framebuffer?.Dispose();
        framebuffer = null;

        if (initialized)
        {
            surface.Dispose();
            await FreeGraphicsResources();
        }

        initialized = false;
        base.OnDetachedFromVisualTree(e);
    }

    private async void InitializeComposition()
    {
        try
        {
            var selfVisual = ElementComposition.GetElementVisual(this);
            if (selfVisual == null)
            {
                return;
            }

            compositor = selfVisual.Compositor;

            surface = compositor.CreateDrawingSurface();
            surfaceVisual = compositor.CreateSurfaceVisual();

            surfaceVisual.Size = new Vector(Bounds.Width, Bounds.Height);

            surfaceVisual.Surface = surface;
            ElementComposition.SetElementChildVisual(this, surfaceVisual);
            var (result, initInfo) = await DoInitialize(compositor, surface);
            info = initInfo;

            initialized = result;
            QueueNextFrame();
        }
        catch (Exception e)
        {
            info = e.Message;
            throw;
        }
    }

    public new void InvalidateVisual()
    {
        QueueNextFrame();
    }

    public void Draw(DrawingSurface texture)
    {
        if (Document == null || SceneRenderer == null) return;

        texture.Canvas.Save();
        var matrix = CalculateTransformMatrix();

        texture.Canvas.SetMatrix(matrix.ToSKMatrix().ToMatrix3X3());

        VecI outputSize = FindOutputSize();

        RectD dirtyBounds = new RectD(0, 0, outputSize.X, outputSize.Y);
        RenderScene(texture, dirtyBounds);

        texture.Canvas.Restore();
    }

    private void RenderScene(DrawingSurface texture, RectD bounds)
    {
        var renderOutput = RenderOutput == "DEFAULT" ? null : RenderOutput;
        DrawCheckerboard(texture, bounds);
        DrawOverlays(texture, bounds, OverlayRenderSorting.Background);
        try
        {
            var tex = Document.SceneTextures[ViewportId];

            bool hasSaved = false;
            int saved = -1;

            var matrix = CalculateTransformMatrix().ToSKMatrix().ToMatrix3X3();
            var cachedTexture = Document.SceneTextures[ViewportId];
            Matrix3X3 matrixDiff = SolveMatrixDiff(matrix, cachedTexture);
            var target = cachedTexture.DrawingSurface;

            if (tex.Size == (VecI)RealDimensions)
            {
                saved = texture.Canvas.Save();
                texture.Canvas.ClipRect(bounds);
                texture.Canvas.SetMatrix(matrixDiff);
                hasSaved = true;
            }
            else
            {
                float scale = CalculateResolutionScale();
                saved = texture.Canvas.Save();
                texture.Canvas.SetMatrix(matrixDiff);
                texture.Canvas.Scale(scale);
                hasSaved = true;
            }


            texture.Canvas.Save();

            texture.Canvas.DrawSurface(target, 0, 0);
            if (hasSaved)
            {
                texture.Canvas.RestoreToCount(saved);
            }
        }
        catch (Exception e)
        {
            texture.Canvas.Clear();
            using Paint paint = new Paint { Color = Colors.White, IsAntiAliased = true };

            using Font defaultSizedFont = Font.CreateDefault();
            defaultSizedFont.Size = 24;

            texture.Canvas.DrawText(new LocalizedString("ERROR_GRAPH"), renderTexture.Size / 2f,
                TextAlign.Center, defaultSizedFont, paint);
        }

        DrawOverlays(texture, bounds, OverlayRenderSorting.Foreground);
    }

    private void DrawCheckerboard(DrawingSurface surface, RectD dirtyBounds)
    {
        if (BackgroundBitmap == null) return;

        RectD operationSurfaceRectToRender = new RectD(0, 0, dirtyBounds.Width, dirtyBounds.Height);
        VecD checkerScale = AutoBackgroundScale
            ? new VecD(ZoomToViewportConverter.ZoomToViewport(16, Scale) * 0.5f)
            : new VecD(CustomBackgroundScaleX, CustomBackgroundScaleY);
        checkerScale = new VecD(Math.Max(0.5, checkerScale.X), Math.Max(0.5, checkerScale.Y));
        checkerPaint?.Shader?.Dispose();
        checkerPaint?.Dispose();
        checkerPaint = new Paint
        {
            Shader = Shader.CreateBitmap(
                BackgroundBitmap,
                TileMode.Repeat, TileMode.Repeat,
                Matrix3X3.CreateScale((float)checkerScale.X, (float)checkerScale.Y)),
            FilterQuality = FilterQuality.None
        };

        surface.Canvas.DrawRect(operationSurfaceRectToRender, checkerPaint);
    }

    private void DrawOverlays(DrawingSurface renderSurface, RectD dirtyBounds, OverlayRenderSorting sorting)
    {
        if (AllOverlays != null)
        {
            foreach (Overlay overlay in AllOverlays)
            {
                try
                {
                    if (!overlay.IsVisible || overlay.OverlayRenderSorting != sorting)
                    {
                        continue;
                    }

                    overlay.PointerPosition = lastMousePosition;

                    overlay.ZoomScale = Scale;

                    if (!overlay.CanRender()) continue;

                    overlay.RenderOverlay(renderSurface.Canvas, dirtyBounds);
                }
                catch (Exception ex)
                {
                    CrashHelper.SendExceptionInfo(ex);
                }
            }
        }
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

    private VecI FindOutputSize()
    {
        VecI outputSize = Document.SizeBindable;

        if (!string.IsNullOrEmpty(RenderOutput))
        {
            if (Document.NodeGraph.CustomRenderOutputs.TryGetValue(RenderOutput, out var node))
            {
                var prop = node?.Inputs.FirstOrDefault(x => x.PropertyName == CustomOutputNode.SizePropertyName);
                if (prop != null)
                {
                    VecI size = Document.NodeGraph.GetComputedPropertyValue<VecI>(prop);
                    if (size.ShortestAxis > 0)
                    {
                        outputSize = size;
                    }
                }
            }
        }

        return outputSize;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        try
        {
            if (AllOverlays != null)
            {
                OverlayPointerArgs args = ConstructPointerArgs(e);
                lastMousePosition = args.Point;

                Cursor finalCursor = DefaultCursor;

                if (capturedOverlay != null)
                {
                    capturedOverlay.MovePointer(args);
                    if (capturedOverlay.IsHitTestVisible)
                    {
                        finalCursor = capturedOverlay.Cursor ?? DefaultCursor;
                    }
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
                        if (overlay.IsHitTestVisible)
                        {
                            finalCursor = overlay.Cursor ?? DefaultCursor;
                        }
                    }
                }

                if (Cursor.ToString() != finalCursor.ToString())
                    Cursor = finalCursor;
                e.Handled = args.Handled;
            }
        }
        catch (Exception ex)
        {
            CrashHelper.SendExceptionInfo(ex);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        try
        {
            if (AllOverlays != null)
            {
                OverlayPointerArgs args = ConstructPointerArgs(e);
                if (capturedOverlay != null)
                {
                    capturedOverlay?.PressPointer(args);
                }
                else
                {
                    foreach (var overlay in AllOverlays)
                    {
                        if (args.Handled) break;
                        if (!overlay.IsVisible) continue;

                        if (!overlay.IsHitTestVisible || !overlay.TestHit(args.Point)) continue;

                        overlay.PressPointer(args);
                    }
                }

                e.Handled = args.Handled;
            }
        }
        catch (Exception ex)
        {
            CrashHelper.SendExceptionInfo(ex);
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        try
        {
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
        catch (Exception ex)
        {
            CrashHelper.SendExceptionInfo(ex);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerExited(e);
        try
        {
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
                    foreach (Overlay overlay in AllOverlays)
                    {
                        if (args.Handled) break;
                        if (!overlay.IsVisible) continue;

                        if (!overlay.IsHitTestVisible || !overlay.TestHit(args.Point)) continue;

                        overlay.ReleasePointer(args);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            CrashHelper.SendExceptionInfo(ex);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        try
        {
            if (AllOverlays != null)
            {
                foreach (Overlay overlay in AllOverlays)
                {
                    if (!overlay.IsVisible) continue;

                    overlay.KeyPressed(e);
                }
            }
        }
        catch (Exception ex)
        {
            CrashHelper.SendExceptionInfo(ex);
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        try
        {
            if (AllOverlays != null)
            {
                foreach (Overlay overlay in AllOverlays)
                {
                    if (!overlay.IsVisible) continue;
                    overlay.KeyReleased(e);
                }
            }
        }
        catch (Exception ex)
        {
            CrashHelper.SendExceptionInfo(ex);
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
            ClickCount = e is PointerPressedEventArgs pressed ? pressed.ClickCount : 0,
        };
    }

    private void FocusOverlay()
    {
        Focus();
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        try
        {
            if (AllOverlays != null)
            {
                foreach (Overlay overlay in AllOverlays)
                {
                    if (!overlay.IsVisible) continue;
                    overlay.FocusChanged(true);
                }
            }
        }
        catch (Exception ex)
        {
            CrashHelper.SendExceptionInfo(ex);
        }
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        try
        {
            if (AllOverlays != null)
            {
                foreach (Overlay overlay in AllOverlays)
                {
                    if (!overlay.IsVisible) continue;
                    overlay.FocusChanged(false);
                }
            }
        }
        catch (Exception ex)
        {
            CrashHelper.SendExceptionInfo(ex);
        }
    }

    private VecD ToCanvasSpace(Point scenePosition)
    {
        Matrix transform = CalculateTransformMatrix();
        Point transformed = transform.Invert().Transform(scenePosition);
        return new VecD(transformed.X, transformed.Y);
    }

    internal Matrix CalculateTransformMatrix()
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
        var resolution = CalculateResolution();
        return (float)resolution.InvertedMultiplier();
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
                overlay.RefreshCursorRequested -= RefreshCursor;
                overlay.FocusRequested -= FocusOverlay;
            }
        }

        if (e.NewItems != null)
        {
            foreach (Overlay overlay in e.NewItems)
            {
                overlay.RefreshRequested += QueueRender;
                overlay.RefreshCursorRequested += RefreshCursor;
                overlay.FocusRequested += FocusOverlay;
            }
        }
    }

    #region Interop

    void UpdateFrame()
    {
        updateQueued = false;
        var root = this.GetVisualRoot();
        if (root == null || !initialized)
        {
            return;
        }

        surfaceVisual.Size = new Vector(Bounds.Width, Bounds.Height);

        if (double.IsNaN(surfaceVisual.Size.X) || double.IsNaN(surfaceVisual.Size.Y))
        {
            return;
        }

        var size = new PixelSize((int)Bounds.Width, (int)Bounds.Height);
        try
        {
            RenderFrame(size);
            info = string.Empty;
        }
        catch (Exception e)
        {
            info = new LocalizedString("ERROR_GPU_RESOURCES_CREATION", e.Message);
            CrashHelper.SendExceptionInfo(e);
            return;
        }
    }

    public void QueueNextFrame()
    {
        if (initialized && !updateQueued && compositor != null && surface is { IsDisposed: false })
        {
            updateQueued = true;
            compositor.RequestCompositionUpdate(update);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == BoundsProperty)
        {
            QueueNextFrame();
        }

        base.OnPropertyChanged(change);
    }


    private Matrix3X3 SolveMatrixDiff(Matrix3X3 matrix, Texture cachedTexture)
    {
        Matrix3X3 old = cachedTexture.DrawingSurface.Canvas.TotalMatrix;
        Matrix3X3 current = matrix;

        Matrix3X3 solveMatrixDiff = current.Concat(old.Invert());
        return solveMatrixDiff;
    }

    private async Task<(bool success, string info)> DoInitialize(Compositor compositor,
        CompositionDrawingSurface surface)
    {
        var interop = await compositor.TryGetCompositionGpuInterop();
        if (interop == null)
        {
            return (false, "Composition interop not available");
        }

        return InitializeGraphicsResources(compositor, surface, interop);
    }

    protected (bool success, string info) InitializeGraphicsResources(Compositor targetCompositor,
        CompositionDrawingSurface compositionDrawingSurface, ICompositionGpuInterop interop)
    {
        try
        {
            resources = IDrawieInteropContext.Current.CreateResources(compositionDrawingSurface, interop);
        }
        catch (Exception e)
        {
            return (false, new LocalizedString("ERROR_GPU_RESOURCES_CREATION", e.Message));
        }

        return (true, string.Empty);
    }

    public override void Render(DrawingContext context)
    {
        if (!string.IsNullOrEmpty(info))
        {
            Point center = new Point(Bounds.Width / 2, Bounds.Height / 2);
            context.DrawText(
                new FormattedText(info, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12,
                    Brushes.White),
                center);
        }
    }

    protected async Task FreeGraphicsResources()
    {
        renderTexture?.Dispose();
        renderTexture = null;

        framebuffer?.Dispose();
        framebuffer = null;

        if (resources != null)
        {
            await resources.DisposeAsync();
        }

        resources = null;
    }

    protected void RenderFrame(PixelSize size)
    {
        if (resources != null && !resources.IsDisposed)
        {
            using var ctx = IDrawieInteropContext.Current.EnsureContext();
            if (size.Width == 0 || size.Height == 0)
            {
                return;
            }

            if (framebuffer == null || lastSize != size)
            {
                resources.CreateTemporalObjects(size);

                VecI sizeVec = new VecI(size.Width, size.Height);

                framebuffer?.Dispose();
                renderTexture?.Dispose();

                framebuffer =
                    DrawingBackendApi.Current.CreateRenderSurface(sizeVec,
                        resources.Texture, SurfaceOrigin.BottomLeft);

                renderTexture = Texture.ForDisplay(sizeVec);

                lastSize = size;
            }

            resources.Render(size, () =>
            {
                framebuffer.Canvas.Clear();
                renderTexture.DrawingSurface.Canvas.Clear();
                Draw(renderTexture.DrawingSurface);
                framebuffer.Canvas.DrawSurface(renderTexture.DrawingSurface, 0, 0);
                framebuffer.Flush();
            });
        }
    }

    #endregion

    public void RefreshCursor()
    {
        Cursor = DefaultCursor;
        if (AllOverlays != null)
        {
            foreach (Overlay overlay in AllOverlays)
            {
                if (!overlay.IsVisible) continue;

                if (overlay.IsHitTestVisible)
                {
                    Cursor = overlay.Cursor ?? DefaultCursor;
                }
            }
        }
    }

    private void QueueRender()
    {
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Render);
        QueueNextFrame();
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

    private static void DocumentChanged(Scene scene, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is DocumentViewModel oldDocumentViewModel)
        {
            oldDocumentViewModel.SizeChanged -= scene.DocumentViewModelOnSizeChanged;
        }

        if (e.NewValue is DocumentViewModel documentViewModel)
        {
            documentViewModel.SizeChanged += scene.DocumentViewModelOnSizeChanged;
            scene.ContentDimensions = scene.Document.GetRenderOutputSize(scene.RenderOutput);
        }
    }

    private void DocumentViewModelOnSizeChanged(object? sender, DocumentSizeChangedEventArgs e)
    {
        ContentDimensions = Document.GetRenderOutputSize(RenderOutput);
    }


    private static void UpdateRenderOutput(Scene scene, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is string newValue)
        {
            scene.ContentDimensions = scene.Document.GetRenderOutputSize(newValue);
            scene.CenterContent();
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
