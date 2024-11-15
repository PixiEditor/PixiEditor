using Avalonia.Input;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Models.Controllers.InputDevice;
internal class MouseOnCanvasEventArgs : EventArgs
{
    public MouseOnCanvasEventArgs(MouseButton button, VecD positionOnCanvas, KeyModifiers keyModifiers)
    {
        Button = button;
        PositionOnCanvas = positionOnCanvas;
        KeyModifiers = keyModifiers;
    }

    public MouseButton Button { get; }
    public VecD PositionOnCanvas { get; }
    public KeyModifiers KeyModifiers { get; }
}
