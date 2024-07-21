using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using PixiEditor.Engine.Helpers;
using SkiaSharp;

namespace PixiEditor.Engine.AvaloniaPlatform.Windowing;

public class PixiEngineWindowImpl : IWindowImpl
{
    public WindowState WindowState { get; set; }
    public Action<WindowState>? WindowStateChanged { get; set; }
    public Action? GotInputWhenDisabled { get; set; }
    public Func<WindowCloseReason, bool>? Closing { get; set; }
    public bool IsClientAreaExtendedToDecorations { get; }
    public Action<bool>? ExtendClientAreaToDecorationsChanged { get; set; }
    public bool NeedsManagedDecorations { get; }
    public Thickness ExtendedMargins { get; }
    public Thickness OffScreenMargin { get; }

    public WindowTransparencyLevel TransparencyLevel
    {
        get => _transparencyLevel;
        private set
        {
            if (_transparencyLevel.Equals(value))
            {
                return;
            }

            _transparencyLevel = value;
            TransparencyLevelChanged?.Invoke(value);
        }
    }

    public Size ClientSize { get; }
    public Size? FrameSize { get; }
    public double RenderScaling { get; } = 1;
    public IEnumerable<object> Surfaces { get; }
    public Action<RawInputEventArgs>? Input { get; set; }
    public Action<Rect>? Paint { get; set; }
    public Action<Size, WindowResizeReason>? Resized { get; set; }
    public Action<double>? ScalingChanged { get; set; }
    public Action<WindowTransparencyLevel>? TransparencyLevelChanged { get; set; }
    public Compositor Compositor { get; }
    public Action? Closed { get; set; }
    public Action? LostFocus { get; set; }
    public AcrylicPlatformCompensationLevels AcrylicCompensationLevels { get; }

    public double DesktopScaling { get; } = 1;
    public PixelPoint Position { get; }
    public Action<PixelPoint>? PositionChanged { get; set; }
    public Action? Deactivated { get; set; }
    public Action? Activated { get; set; }
    public IPlatformHandle Handle { get; }
    public Size MaxAutoSizeHint { get; }
    public IScreenImpl Screen { get; }

    private IInputRoot? _inputRoot;
    private ICursorImpl? _cursor;
    private WindowTransparencyLevel _transparencyLevel;
    private Window _underlyingWindow;

    public PixiEngineWindowImpl(Window underlyingWindow, PixiEngineTopLevel topLevel)
    {
        _underlyingWindow = underlyingWindow;
        Compositor = PixiEnginePlatform.Compositor;
        
        topLevel.Impl.Surface = new PixiEngineSkiaSurface(underlyingWindow.FramebufferSurface, topLevel.RenderScaling);
        topLevel.Impl.SetRenderSize(new Size(underlyingWindow.Size.X, underlyingWindow.Size.Y));
        ClientSize = new Size(underlyingWindow.Size.X, underlyingWindow.Size.Y);
        
        topLevel.Prepare();
        topLevel.StartRendering();
        
        topLevel.Content = underlyingWindow;
    }

    object? IOptionalFeatureProvider.TryGetFeature(Type featureType) { return null; }

    void ITopLevelImpl.SetInputRoot(IInputRoot inputRoot)
    {
        _inputRoot = inputRoot;
    }

    public Point PointToClient(PixelPoint point)
    {
        return point.ToPoint(RenderScaling);
    }

    public PixelPoint PointToScreen(Point point)
    {
        return PixelPoint.FromPoint(point, RenderScaling);
    }

    public void SetCursor(ICursorImpl? cursor)
    {
        _cursor = cursor;
    }

    public IPopupImpl? CreatePopup()
    {
        return null;
    }

    public void SetTransparencyLevelHint(IReadOnlyList<WindowTransparencyLevel> transparencyLevels)
    {
        foreach (var transparencyLevel in transparencyLevels)
        {
            if (transparencyLevel == WindowTransparencyLevel.Transparent ||
                transparencyLevel == WindowTransparencyLevel.None)
            {
                TransparencyLevel = transparencyLevel;
                return;
            }
        }
    }

    public void SetFrameThemeVariant(PlatformThemeVariant themeVariant)
    {
    }

    public void Show(bool activate, bool isDialog)
    {
        _underlyingWindow.Show();
    }

    public void Hide()
    {
        _underlyingWindow.Hide();
    }

    public void Activate()
    {
        _underlyingWindow.Activate();
    }

    public void SetTopmost(bool value)
    {
        _underlyingWindow.TopMost = value;
    }

    public void SetTitle(string? title)
    {
        _underlyingWindow.Title = title;
    }

    public void SetParent(IWindowImpl parent)
    {
    }

    public void SetEnabled(bool enable)
    {
    }

    public void SetSystemDecorations(SystemDecorations enabled)
    {
    }

    public void SetIcon(IWindowIconImpl? icon)
    {
    }

    public void ShowTaskbarIcon(bool value)
    {
    }

    public void CanResize(bool value)
    {
    }

    public void BeginMoveDrag(PointerPressedEventArgs e)
    {
    }

    public void BeginResizeDrag(WindowEdge edge, PointerPressedEventArgs e)
    {
    }

    public void Resize(Size clientSize, WindowResizeReason reason = WindowResizeReason.Application)
    {
    }

    public void Move(PixelPoint point)
    {
    }

    public void SetMinMaxSize(Size minSize, Size maxSize)
    {
    }

    public void SetExtendClientAreaToDecorationsHint(bool extendIntoClientAreaHint)
    {
    }

    public void SetExtendClientAreaChromeHints(ExtendClientAreaChromeHints hints)
    {
    }

    public void SetExtendClientAreaTitleBarHeightHint(double titleBarHeight)
    {
    }

    public void Dispose()
    {
    }
}
