using Avalonia.Input;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Models.Controllers.InputDevice;
internal class MouseOnCanvasEventArgs : EventArgs
{
    public MouseOnCanvasEventArgs(MouseButton button, VecD positionOnCanvas)
    {
        Button = button;
        PositionOnCanvas = positionOnCanvas;
    }

    public MouseButton Button { get; }
    public VecD PositionOnCanvas { get; }
}
