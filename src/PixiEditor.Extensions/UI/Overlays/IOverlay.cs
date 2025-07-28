using Avalonia.Input;
using Drawie.Numerics;

namespace PixiEditor.Extensions.UI.Overlays;

public delegate void PointerEvent(OverlayPointerArgs args);
public delegate void KeyEvent(Key key, KeyModifiers modifiers);
public interface IOverlay
{
    public void EnterPointer(OverlayPointerArgs args);
    public void ExitPointer(OverlayPointerArgs args);
    public void MovePointer(OverlayPointerArgs args);
    public void PressPointer(OverlayPointerArgs args);
    public void ReleasePointer(OverlayPointerArgs args);
    public void Refresh();
    public bool TestHit(VecD point);

    public event PointerEvent PointerEnteredOverlay;
    public event PointerEvent PointerExitedOverlay;
    public event PointerEvent PointerMovedOverlay;
    public event PointerEvent PointerPressedOverlay;
    public event PointerEvent PointerReleasedOverlay;
    public Cursor Cursor { get; set; } // TODO: Non Avalonia Cursor struct
}
