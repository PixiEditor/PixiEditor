using System.Windows.Input;
using ChunkyImageLib.DataHolders;

namespace PixiEditor.Models.Events;
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
