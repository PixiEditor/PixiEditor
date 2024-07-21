using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;

namespace PixiEditor.Engine.AvaloniaPlatform;

internal sealed class PixiEngineTopLevelImpl : ITopLevelImpl
{
    public Size ClientSize { get; }
    public Size? FrameSize { get; }
    public double RenderScaling { get; }
    public IEnumerable<object> Surfaces { get; }
    public Action<RawInputEventArgs>? Input { get; set; }
    public Action<Rect>? Paint { get; set;  }
    public Action<Size, WindowResizeReason>? Resized { get; set; }
    public Action<double>? ScalingChanged { get; set; }
    public Action<WindowTransparencyLevel>? TransparencyLevelChanged { get; set; }
    public Compositor Compositor { get; }
    public Action? Closed { get; set; }
    public Action? LostFocus { get; set; }
    public WindowTransparencyLevel TransparencyLevel { get; }
    public AcrylicPlatformCompensationLevels AcrylicCompensationLevels { get; }

    public object? TryGetFeature(Type featureType)
    {
        throw new NotImplementedException();
    }

    public void SetInputRoot(IInputRoot inputRoot)
    {
        throw new NotImplementedException();
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

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
