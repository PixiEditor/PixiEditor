using Avalonia.Input;

namespace PixiEditor.Extensions.UI.Overlays;

public interface IOverlayPointer
{
    public PointerType Type { get; }
    public void Capture(IOverlay? overlay);
}
