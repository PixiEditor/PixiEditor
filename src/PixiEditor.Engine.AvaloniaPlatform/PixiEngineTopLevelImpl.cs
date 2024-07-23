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
    public double RenderScaling { get; private set; } = 1;
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

    public PixiEngineSkiaSurface Surface
    {
        get => _surface;
        set
        {
            _surface = value;
            Surfaces = new[] { _surface };
        }
    }
    
    public PixiEngineGraphics Graphics { get; }

    private PixiEngineSkiaSurface _surface;

    private IInputRoot? _inputRoot;
    private ICursorImpl _cursor;
    private WindowTransparencyLevel _transparencyLevel;
    private PixelSize _renderSize;

    public PixiEngineTopLevelImpl(Compositor compositor, PixiEngineGraphics graphics)
    {
        Compositor = compositor;
        Graphics = graphics;
    }
    
    public void OnDraw(Rect rect)
    {
        Paint?.Invoke(rect);
    }
    
    public void SetRenderSize(PixelSize renderSize, double renderScaling)
    {
        var hasScalingChanged = RenderScaling != renderScaling;
        if (_renderSize == renderSize && !hasScalingChanged)
            return;

        var oldClientSize = ClientSize;
        var unclampedClientSize = renderSize.ToSize(renderScaling);

        ClientSize = new Size(Math.Max(unclampedClientSize.Width, 0.0), Math.Max(unclampedClientSize.Height, 0.0));
        RenderScaling = renderScaling;

        if (_renderSize != renderSize) 
        {
            _renderSize = renderSize;

            if (_surface is not null) 
            {
                _surface.Dispose();
                _surface = null;
            }

            _surface = CreateSurface();
        }

        if (hasScalingChanged) 
        {
            if (_surface != null)
                _surface.RenderScaling = RenderScaling;
            
            ScalingChanged?.Invoke(RenderScaling);
        }

        if (oldClientSize != ClientSize)
            Resized?.Invoke(ClientSize, hasScalingChanged ? WindowResizeReason.DpiChange : WindowResizeReason.Unspecified);
    }

    private PixiEngineSkiaSurface CreateSurface()
    {
        return new PixiEngineSkiaSurface(Graphics.GetSharedContext().CreateSurface(_renderSize.Width, _renderSize.Height), RenderScaling);
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
