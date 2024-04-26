using Avalonia.Input;
using PixiEditor.AvaloniaUI.Views.Overlays.Pointers;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Overlays;

public struct OverlayPointerArgs
{
    public VecD Point { get; set; }
    public KeyModifiers Modifiers { get; set; }
    public MouseButton PointerButton { get; set; }
    public MouseButton InitialPressMouseButton { get; set; }

    public IOverlayPointer Pointer { get; set; }
}
