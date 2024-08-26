using Avalonia;
using Avalonia.Input;

namespace PixiEditor.Helpers;

public static class PointerHelpers
{
    public static MouseButton GetMouseButton(this PointerEventArgs e, Visual visual)
    {
        return e.GetCurrentPoint(visual).Properties.PointerUpdateKind switch
        {
            PointerUpdateKind.LeftButtonPressed => MouseButton.Left,
            PointerUpdateKind.RightButtonPressed => MouseButton.Right,
            PointerUpdateKind.MiddleButtonPressed => MouseButton.Middle,
            _ => MouseButton.None
        };
    }
}
