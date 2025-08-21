using Avalonia.Input;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Models.Controllers.InputDevice;

internal class MouseOnCanvasEventArgs : EventArgs
{
    public MouseButton Button { get; }
    public VecD PositionOnCanvas { get; }
    public KeyModifiers KeyModifiers { get; }
    public bool Handled { get; set; }
    public int ClickCount { get; set; } = 1;
    public PointerPointProperties Properties { get; }

    public MouseOnCanvasEventArgs(MouseButton button, VecD positionOnCanvas, KeyModifiers keyModifiers, int clickCount,
        PointerPointProperties properties)
    {
        Button = button;
        PositionOnCanvas = positionOnCanvas;
        KeyModifiers = keyModifiers;
        ClickCount = clickCount;
        Properties = properties;
    }
}
