using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Models.Controllers;

internal class MouseInputFilter
{
    public EventHandler<MouseButton> OnMouseDown;
    public EventHandler OnMouseMove;
    public EventHandler<MouseButton> OnMouseUp;


    private Dictionary<MouseButton, MouseButtonState> buttonStates = new()
    {
        [MouseButton.Left] = MouseButtonState.Released,
        [MouseButton.Right] = MouseButtonState.Released,
        [MouseButton.Middle] = MouseButtonState.Released,
    };

    public void MouseDown(object args) => MouseDown(((MouseButtonEventArgs)args).ChangedButton);
    public void MouseDown(MouseButton button)
    {
        if (button is MouseButton.XButton1 or MouseButton.XButton2)
            return;
        if (buttonStates[button] == MouseButtonState.Pressed)
            return;
        buttonStates[button] = MouseButtonState.Pressed;

        OnMouseDown?.Invoke(this, button);
    }

    public void MouseMove(object args) => OnMouseMove?.Invoke(this, EventArgs.Empty);
    public void MouseMove(MouseEventArgs args) => OnMouseMove?.Invoke(this, EventArgs.Empty);

    public void MouseUp(object args) => MouseUp(((MouseButtonEventArgs)args).ChangedButton);
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