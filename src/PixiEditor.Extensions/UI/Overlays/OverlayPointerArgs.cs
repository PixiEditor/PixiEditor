using Avalonia.Input;
using PixiEditor.Numerics;

namespace PixiEditor.Extensions.UI.Overlays;

public class OverlayPointerArgs
{
    public VecD Point { get; set; }
    public KeyModifiers Modifiers { get; set; }
    public MouseButton PointerButton { get; set; }
    public MouseButton InitialPressMouseButton { get; set; }
    public IOverlayPointer Pointer { get; set; }
    public bool Handled { get; set; }
}
