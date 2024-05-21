using Avalonia.Input;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Models.Controllers.InputDevice;
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
