using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

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
