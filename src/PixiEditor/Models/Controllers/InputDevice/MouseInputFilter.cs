﻿using System.Collections.Generic;
using Avalonia.Input;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using Point = Avalonia.Point;

namespace PixiEditor.Models.Controllers.InputDevice;
#nullable enable
internal class MouseInputFilter
{
    public EventHandler<MouseOnCanvasEventArgs> OnMouseDown;
    public EventHandler<VecD> OnMouseMove;
    public EventHandler<MouseOnCanvasEventArgs> OnMouseUp;
    public EventHandler<ScrollOnCanvasEventArgs> OnMouseWheel;


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

        if (button is MouseButton.XButton1 or MouseButton.XButton2 or MouseButton.None)
            return;
        if (buttonStates[button])
            return;
        buttonStates[button] = true;

        OnMouseDown?.Invoke(this, args);
    }

    public void MouseMoveInlet(object args) => OnMouseMove?.Invoke(this, ((MouseOnCanvasEventArgs)args).PositionOnCanvas);
    public void MouseUpInlet(object args) => MouseUpInlet(((MouseOnCanvasEventArgs)args));
    public void MouseUpInlet(object? sender, Point p, MouseButton button) => MouseUpInlet(button);
    public void MouseUpInlet(MouseOnCanvasEventArgs args)
    {
        if (args.Button is MouseButton.XButton1 or MouseButton.XButton2 or MouseButton.None)
            return;
        if (!buttonStates[args.Button])
            return;
        buttonStates[args.Button] = false;

        OnMouseUp?.Invoke(this, args);
    }

    public void MouseWheelInlet(ScrollOnCanvasEventArgs e) => OnMouseWheel?.Invoke(this, e);

    public void DeactivatedInlet(object? sender, EventArgs e)
    {
        MouseOnCanvasEventArgs argsLeft = new(MouseButton.Left, VecD.Zero, KeyModifiers.None, 0);
        MouseUpInlet(argsLeft);
        
        MouseOnCanvasEventArgs argsMiddle = new(MouseButton.Middle, VecD.Zero, KeyModifiers.None, 0);
        MouseUpInlet(argsMiddle);
        
        MouseOnCanvasEventArgs argsRight = new(MouseButton.Right, VecD.Zero, KeyModifiers.None, 0);
        MouseUpInlet(argsRight);
    }
}
