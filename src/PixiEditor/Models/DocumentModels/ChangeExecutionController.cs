using System.Windows.Input;
using ChunkyImageLib.DataHolders;

namespace PixiEditor.Models.DocumentModels;
#nullable enable
internal class ChangeExecutionController
{
    public event EventHandler<VecI>? PixelMousePositionChanged;
    public event EventHandler<(double, double)>? PreciseMousePositionChanged;
    public event EventHandler<(Key, KeyStates)>? KeyStateChanged;

    public MouseButtonState LeftMouseState { get; private set; }

    public bool IsShiftDown => keyboardState.ContainsKey(Key.LeftShift) ? keyboardState[Key.LeftShift] == KeyStates.Down : false;
    public bool IsCtrlDown => keyboardState.ContainsKey(Key.LeftCtrl) ? keyboardState[Key.LeftCtrl] == KeyStates.Down : false;
    public bool IsAltDown => keyboardState.ContainsKey(Key.LeftAlt) ? keyboardState[Key.LeftAlt] == KeyStates.Down : false;

    public VecI LastPixelPosition => new(lastPixelX, lastPixelY);

    private int lastPixelX;
    private int lastPixelY;

    private Dictionary<Key, KeyStates> keyboardState = new();
    //private ToolViewModel? currentTool = null;
    //private UpdateableChangeSession? currentSession = null;
    /*
    private void TryStartToolSession(ToolViewModel tool, double mouseXOnCanvas, double mouseYOnCanvas)
    {
        if (currentSession is not null)
            return;
        currentSession = new(tool, mouseXOnCanvas, mouseYOnCanvas, keyboardState);
        SessionStarted?.Invoke(this, currentSession);
    }

    private void TryStopToolSession()
    {
        if (currentSession is null)
            return;
        currentSession.EndSession(keyboardState);
        SessionEnded?.Invoke(this, currentSession);
        currentSession = null;
    }

    public void OnKeyDown(Key key)
    {
        key = ConvertRightKeys(key);
        UpdateKeyState(key, KeyStates.Down);
        currentSession?.OnKeyDown(key);
        KeyStateChanged?.Invoke(this, (key, KeyStates.Down));
    }

    public void OnKeyUp(Key key)
    {
        key = ConvertRightKeys(key);
        UpdateKeyState(key, KeyStates.None);
        currentSession?.OnKeyUp(key);
        KeyStateChanged?.Invoke(this, (key, KeyStates.None));
    }

    private void UpdateKeyState(Key key, KeyStates state)
    {
        key = ConvertRightKeys(key);
        if (!keyboardState.ContainsKey(key))
            keyboardState.Add(key, state);
        else
            keyboardState[key] = state;
    }

    private Key ConvertRightKeys(Key key)
    {
        if (key == Key.RightAlt)
            return Key.LeftAlt;
        if (key == Key.RightCtrl)
            return Key.LeftCtrl;
        if (key == Key.RightShift)
            return Key.LeftShift;
        return key;
    }

    public void ForceStopActiveSessionIfAny() => TryStopToolSession();
    
    public void OnToolChange(ToolViewModel tool)
    {
        currentTool = tool;
        TryStopToolSession();
    }

    public void OnMouseMove(double newCanvasX, double newCanvasY)
    {
        //update internal state

        var newX = (int)Math.Floor(newCanvasX);
        var newY = (int)Math.Floor(newCanvasY);
        var pixelPosChanged = false;
        if (lastPixelX != newX || lastPixelY != newY)
        {
            lastPixelX = newX;
            lastPixelY = newY;
            pixelPosChanged = true;
        }


        //call session events
        if (currentSession != null && pixelPosChanged)
            currentSession.OnPixelPositionChange(new(newX, newY));

        //call internal events
        PreciseMousePositionChanged?.Invoke(this, (newCanvasX, newCanvasY));
        if (pixelPosChanged)
            PixelMousePositionChanged?.Invoke(this, new MouseMovementEventArgs(new VecI(newX, newY)));
    }

    public void OnLeftMouseButtonDown(double canvasPosX, double canvasPosY)
    {
        //update internal state
        LeftMouseState = MouseButtonState.Pressed;

        //call session events

        if (currentTool == null)
            throw new Exception("Current tool must not be null here");
        TryStartToolSession(currentTool, canvasPosX, canvasPosY);
    }

    public void OnLeftMouseButtonUp()
    {
        //update internal state
        LeftMouseState = MouseButtonState.Released;

        //call session events
        TryStopToolSession();
    }*/
}
