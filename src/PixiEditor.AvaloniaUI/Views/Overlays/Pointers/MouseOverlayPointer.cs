using Avalonia.Input;
using PixiEditor.AvaloniaUI.Views.Visuals;

namespace PixiEditor.AvaloniaUI.Views.Overlays.Pointers;

internal class MouseOverlayPointer : IOverlayPointer
{
    IPointer pointer;
    private Action<Overlay?, IPointer> captureAction;

    public MouseOverlayPointer(IPointer pointer, Action<Overlay?, IPointer> captureAction)
    {
        this.pointer = pointer;
        this.captureAction = captureAction;
    }

    public void Capture(Overlay? overlay)
    {
        captureAction(overlay, pointer);
    }
}
