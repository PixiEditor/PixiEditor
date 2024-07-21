using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;

namespace PixiEditor.Engine.AvaloniaPlatform;

internal sealed class PixiEngineTopLevelImpl : ITopLevelImpl
{
    public Size ClientSize { get; private set; }
    public Size? FrameSize { get; private set; }
    public double RenderScaling { get; } = 1;
    public IEnumerable<object> Surfaces { get; private set; }
    public Action<RawInputEventArgs>? Input { get; set; }
    public Action<Rect>? Paint { get; set; }
    public Action<Size, WindowResizeReason>? Resized { get; set; }
    public Action<double>? ScalingChanged { get; set; }
    public Action<WindowTransparencyLevel>? TransparencyLevelChanged { get; set; }
    public Compositor Compositor { get; }
    public Action? Closed { get; set; }
    public Action? LostFocus { get; set; }
    public AcrylicPlatformCompensationLevels AcrylicCompensationLevels { get; }

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

    private PixiEngineSkiaSurface _surface;

    private IInputRoot? _inputRoot;
    private ICursorImpl _cursor;
    private WindowTransparencyLevel _transparencyLevel;

    public PixiEngineTopLevelImpl(Compositor compositor)
    {
        Compositor = compositor;
    }
    
    public void SetRenderSize(Size size)
    {
        ClientSize = size;
        FrameSize = size;
    }

    public object? TryGetFeature(Type featureType)
    {
        return null;
    }

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

    IPopupImpl? ITopLevelImpl.CreatePopup()
    {
        return null;
    }

    void ITopLevelImpl.SetTransparencyLevelHint(IReadOnlyList<WindowTransparencyLevel> transparencyLevels)
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

    public void SetFrameThemeVariant(PlatformThemeVariant themeVariant) { }

    public void Dispose()
    {
       Closed?.Invoke(); 
    }
}
