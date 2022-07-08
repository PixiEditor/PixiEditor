using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.DocumentModels;
#nullable enable
internal class ChangeExecutionController
{
    public MouseButtonState LeftMouseState { get; private set; }
    public VecI LastPixelPosition => lastPixelPos;
    public VecD LastPrecisePosition => lastPrecisePos;
    public bool IsChangeActive => currentSession is not null;

    private readonly DocumentViewModel document;
    private readonly DocumentHelpers helpers;

    private VecI lastPixelPos;
    private VecD lastPrecisePos;

    private UpdateableChangeExecutor? currentSession = null;

    public ChangeExecutionController(DocumentViewModel document, DocumentHelpers helpers)
    {
        this.document = document;
        this.helpers = helpers;
    }

    public bool TryStartUpdateableChange<T>()
        where T : UpdateableChangeExecutor, new()
    {
        if (currentSession is not null)
            return false;
        T executor = new T();
        executor.Initialize(document, helpers, this, EndChange);
        if (executor.Start().IsT0)
        {
            currentSession = executor;
            return true;
        }
        return false;
    }

    private void EndChange(UpdateableChangeExecutor executor)
    {
        if (executor != currentSession)
            throw new InvalidOperationException();
        currentSession = null;
    }

    public bool TryStopActiveUpdateableChange()
    {
        if (currentSession is null)
            return false;
        currentSession.ForceStop();
        currentSession = null;
        return true;
    }

    public void OnKeyDown(Key key)
    {
        key = ConvertRightKeys(key);
        currentSession?.OnKeyDown(key);
    }

    public void OnKeyUp(Key key)
    {
        key = ConvertRightKeys(key);
        currentSession?.OnKeyUp(key);
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

    public void OnMouseMove(VecD newCanvasPos)
    {
        //update internal state
        VecI newPixelPos = (VecI)newCanvasPos.Floor();
        bool pixelPosChanged = false;
        if (lastPixelPos != newPixelPos)
        {
            lastPixelPos = newPixelPos;
            pixelPosChanged = true;
        }
        lastPrecisePos = newCanvasPos;

        //call session events
        if (currentSession is not null)
        {
            if (pixelPosChanged)
                currentSession.OnPixelPositionChange(newPixelPos);
            currentSession.OnPrecisePositionChange(newCanvasPos);
        }
    }

    public void OnLeftMouseButtonDown(VecD canvasPos)
    {
        //update internal state
        LeftMouseState = MouseButtonState.Pressed;

        //call session event
        currentSession?.OnLeftMouseButtonDown(canvasPos);
    }

    public void OnLeftMouseButtonUp()
    {
        //update internal state
        LeftMouseState = MouseButtonState.Released;

        //call session events
        currentSession?.OnLeftMouseButtonUp();
    }
}
