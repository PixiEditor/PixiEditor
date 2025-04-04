using Avalonia.Input;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Models.Controllers.InputDevice;

internal class ScrollOnCanvasEventArgs : EventArgs
{
    public VecD Delta { get; }
    public VecD PositionOnCanvas { get; }
    public KeyModifiers KeyModifiers { get; }
    public bool Handled { get; set; }


    public ScrollOnCanvasEventArgs(VecD delta, VecD positionOnCanvas, KeyModifiers keyModifiers)
    {
        Delta = delta;
        PositionOnCanvas = positionOnCanvas;
        KeyModifiers = keyModifiers;
    }
}
