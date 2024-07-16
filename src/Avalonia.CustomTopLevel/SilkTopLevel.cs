using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;

namespace Avalonia.CustomTopLevel;

public class SilkTopLevel : ITopLevelImpl
{
    
    public Size ClientSize { get; }
    public Size? FrameSize { get; }
    public double RenderScaling { get; }
    public IEnumerable<object> Surfaces { get; }
    public Action<RawInputEventArgs>? Input { get; }
    public Action<Rect>? Paint { get; }
    public Action<Size, WindowResizeReason>? Resized { get; }
    public Action<double>? ScalingChanged { get; }
    public Action<WindowTransparencyLevel>? TransparencyLevelChanged { get; }
    public Compositor Compositor { get; }
    public Action? Closed { get; }
    public Action? LostFocus { get; }
    public WindowTransparencyLevel TransparencyLevel { get; }
    public AcrylicPlatformCompensationLevels AcrylicCompensationLevels { get; }
    
    public object? TryGetFeature(Type featureType)
    {
        
    }

    public void Dispose()
    {
    }

    public void SetInputRoot(IInputRoot inputRoot)
    {
        
    }

    public Point PointToClient(PixelPoint point)
    {
        throw new NotImplementedException();
    }

    public PixelPoint PointToScreen(Point point)
    {
        throw new NotImplementedException();
    }

    public void SetCursor(ICursorImpl? cursor)
    {
        throw new NotImplementedException();
    }

    public IPopupImpl? CreatePopup()
    {
        throw new NotImplementedException();
    }

    public void SetTransparencyLevelHint(IReadOnlyList<WindowTransparencyLevel> transparencyLevels)
    {
        throw new NotImplementedException();
    }

    public void SetFrameThemeVariant(PlatformThemeVariant themeVariant)
    {
        throw new NotImplementedException();
    }
}
