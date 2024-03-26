using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Models.Controllers.InputDevice;
#nullable enable
internal class MouseInputFilter
{
    public EventHandler<MouseOnCanvasEventArgs> OnMouseDown;
    public EventHandler<VecD> OnMouseMove;
    public EventHandler<MouseButton> OnMouseUp;


    private Dictionary<MouseButton, bool> buttonStates = new()
    {
        [MouseButton.Left] = false,
        [MouseButton.Right] = false,
        [MouseButton.Middle] = false,
    };

    public void MouseDownInlet(object args) => MouseDownInlet((MouseOnCanvasEventArgs)args);
    public void MouseDownInlet(MouseOnCanvasEventArgs args)
    {
        var button = args.Button;

        if (button is MouseButton.XButton1 or MouseButton.XButton2)
            return;
        if (buttonStates[button])
            return;
        buttonStates[button] = true;

        OnMouseDown?.Invoke(this, args);
    }

    public void MouseMoveInlet(object args) => OnMouseMove?.Invoke(this, ((MouseOnCanvasEventArgs)args).PositionOnCanvas);

    public void MouseUpInlet(object args) => MouseUpInlet(((MouseOnCanvasEventArgs)args).Button);
    public void MouseUpInlet(object? sender, Point p, MouseButton button) => MouseUpInlet(button);
    public void MouseUpInlet(MouseButton button)
    {
        if (button is MouseButton.XButton1 or MouseButton.XButton2 or MouseButton.None)
            return;
        if (!buttonStates[button])
            return;
        buttonStates[button] = false;

        OnMouseUp?.Invoke(this, button);
    }

    public void DeactivatedInlet(object? sender, EventArgs e)
    {
        MouseUpInlet(MouseButton.Left);
        MouseUpInlet(MouseButton.Middle);
        MouseUpInlet(MouseButton.Right);
    }
}
