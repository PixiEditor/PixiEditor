using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using SkiaSharp;

namespace PixiEditor.Models.Controllers;

public class ToolSession
{
    private List<Coordinates> mouseMovement = new();
    private bool ended = false;

    public IReadOnlyList<Coordinates> MouseMovement => mouseMovement;
    public Tool Tool { get; }

    public bool IsCtrlDown { get; private set; }
    public bool IsShiftDown { get; private set; }
    public bool IsAltDown { get; private set; }

    private int _smallestX = int.MaxValue;
    private int _smallestY = int.MaxValue;
    private int _biggestX = int.MinValue;
    private int _biggestY = int.MinValue;

    public ToolSession(
        Tool tool,
        double mouseXOnCanvas,
        double mouseYOnCanvas,
        IReadOnlyDictionary<Key, KeyStates> keyboardStates)
    {
        if (tool == null)
            throw new ArgumentNullException(nameof(tool));
        Tool = tool;

        Tool.Session = this;
        InvokeKeyboardEvents(keyboardStates);
        int x = (int)Math.Floor(mouseXOnCanvas);
        int y = (int)Math.Floor(mouseYOnCanvas);
        mouseMovement.Add(new(x, y));

        UpdateMinMax(x, y);

        Tool.BeforeUse();
    }

    private void InvokeKeyboardEvents(IReadOnlyDictionary<Key, KeyStates> keyboardStates)
    {
        foreach (var pair in keyboardStates)
        {
            if (pair.Value == KeyStates.None)
                OnKeyUp(pair.Key);
            else if (pair.Value == KeyStates.Down)
                OnKeyDown(pair.Key);
        }
    }

    public void EndSession(IReadOnlyDictionary<Key, KeyStates> keyboardStates)
    {
        if (ended)
            throw new Exception("Session has ended already");
        ended = true;

        Tool.AfterUse(SKRectI.Create(
            _smallestX,
            _smallestY,
            _biggestX - _smallestX + 1,
            _biggestY - _smallestY + 1));
        InvokeReleaseKeyboardEvents(keyboardStates);
        Tool.Session = null;
    }

    private void InvokeReleaseKeyboardEvents(IReadOnlyDictionary<Key, KeyStates> keyboardStates)
    {
        foreach (var pair in keyboardStates)
        {
            if (pair.Value == KeyStates.Down)
                OnKeyUp(pair.Key);
        }
    }

    public void OnKeyDown(Key key)
    {
        if (key == Key.LeftCtrl)
            IsCtrlDown = true;
        else if (key == Key.LeftShift)
            IsShiftDown = true;
        else if (key == Key.LeftAlt)
            IsAltDown = true;

        Tool.OnKeyDown(key);
    }

    public void OnKeyUp(Key key)
    {
        if (key == Key.LeftCtrl)
            IsCtrlDown = false;
        else if (key == Key.LeftShift)
            IsShiftDown = false;
        else if (key == Key.LeftAlt)
            IsAltDown = false;

        Tool.OnKeyUp(key);
    }

    public void OnPixelPositionChange(Coordinates pos)
    {
        UpdateMinMax(pos.X, pos.Y);
        mouseMovement.Add(pos);
    }

    private void UpdateMinMax(int x, int y)
    {
        _smallestX = Math.Min(_smallestX, x);
        _smallestY = Math.Min(_smallestY, y);
        _biggestX = Math.Max(_biggestX, x);
        _biggestY = Math.Max(_biggestY, y);
    }
}