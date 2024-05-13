using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Events;
using PixiEditor.Numerics;
using Point = System.Windows.Point;

namespace PixiEditor.Models.Controllers;
#nullable enable
internal class MouseInputFilter
{
    public EventHandler<MouseOnCanvasEventArgs> OnMouseDown;
    public EventHandler<VecD> OnMouseMove;
    public EventHandler<MouseButton> OnMouseUp;


    private Dictionary<MouseButton, MouseButtonState> buttonStates = new()
    {
        [MouseButton.Left] = MouseButtonState.Released,
        [MouseButton.Right] = MouseButtonState.Released,
        [MouseButton.Middle] = MouseButtonState.Released,
    };

    public void MouseDownInlet(object args) => MouseDownInlet((MouseOnCanvasEventArgs)args);
    public void MouseDownInlet(MouseOnCanvasEventArgs args)
    {
        var button = args.Button;

        if (button is MouseButton.XButton1 or MouseButton.XButton2)
            return;
        if (buttonStates[button] == MouseButtonState.Pressed)
            return;
        buttonStates[button] = MouseButtonState.Pressed;

        OnMouseDown?.Invoke(this, args);
    }

    public void MouseMoveInlet(object args) => OnMouseMove?.Invoke(this, (VecD)args);

    public void MouseUpInlet(object args) => MouseUpInlet((MouseButton)args);
    public void MouseUpInlet(object? sender, Point p, MouseButton button) => MouseUpInlet(button);
    public void MouseUpInlet(MouseButton button)
    {
        if (button is MouseButton.XButton1 or MouseButton.XButton2)
            return;
        if (buttonStates[button] == MouseButtonState.Released)
            return;
        buttonStates[button] = MouseButtonState.Released;

        OnMouseUp?.Invoke(this, button);
    }

    public void DeactivatedInlet(object? sender, EventArgs e)
    {
        MouseUpInlet(MouseButton.Left);
        MouseUpInlet(MouseButton.Middle);
        MouseUpInlet(MouseButton.Right);
    }
}
