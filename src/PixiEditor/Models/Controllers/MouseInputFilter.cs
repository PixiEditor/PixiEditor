using System.Windows;
using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.Events;

namespace PixiEditor.Models.Controllers;

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

    public void MouseDown(object args) => MouseDown((MouseOnCanvasEventArgs)args);
    public void MouseDown(MouseOnCanvasEventArgs args)
    {
        var button = args.Button;

        if (button is MouseButton.XButton1 or MouseButton.XButton2)
            return;
        if (buttonStates[button] == MouseButtonState.Pressed)
            return;
        buttonStates[button] = MouseButtonState.Pressed;

        OnMouseDown?.Invoke(this, args);
    }

    public void MouseMove(object args) => OnMouseMove?.Invoke(this, (VecD)args);

    public void MouseUp(object args) => MouseUp((MouseButton)args);
    public void MouseUp(object sender, Point p, MouseButton button) => MouseUp(button);
    public void MouseUp(MouseButton button)
    {
        if (button is MouseButton.XButton1 or MouseButton.XButton2)
            return;
        if (buttonStates[button] == MouseButtonState.Released)
            return;
        buttonStates[button] = MouseButtonState.Released;

        OnMouseUp?.Invoke(this, button);
    }
}
