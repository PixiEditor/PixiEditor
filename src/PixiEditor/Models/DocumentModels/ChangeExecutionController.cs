using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.DocumentModels;
#nullable enable
internal class ChangeExecutionController
{
    public MouseButtonState LeftMouseState { get; private set; }
    public ShapeCorners LastTransformState { get; private set; }
    public VecI LastPixelPosition => lastPixelPos;
    public VecD LastPrecisePosition => lastPrecisePos;
    public float LastOpacityValue = 1f;
    public bool IsChangeActive => currentSession is not null;

    private readonly DocumentViewModel document;
    private readonly DocumentInternalParts internals;

    private VecI lastPixelPos;
    private VecD lastPrecisePos;

    private UpdateableChangeExecutor? currentSession = null;

    public ChangeExecutionController(DocumentViewModel document, DocumentInternalParts internals)
    {
        this.document = document;
        this.internals = internals;
    }

    public ExecutorType GetCurrentExecutorType()
    {
        if (currentSession is null)
            return ExecutorType.None;
        return currentSession.Type;
    }

    public bool TryStartExecutor<T>(bool force = false)
        where T : UpdateableChangeExecutor, new()
    {
        if (currentSession is not null && !force)
            return false;
        if (force)
            currentSession?.ForceStop();
        T executor = new T();
        executor.Initialize(document, internals, this, EndExecutor);
        if (executor.Start() == ExecutionState.Success)
        {
            currentSession = executor;
            return true;
        }
        return false;
    }

    public bool TryStartExecutor(UpdateableChangeExecutor brandNewExecutor, bool force = false)
    {
        if (currentSession is not null && !force)
            return false;
        if (force)
            currentSession?.ForceStop();
        brandNewExecutor.Initialize(document, internals, this, EndExecutor);
        if (brandNewExecutor.Start() == ExecutionState.Success)
        {
            currentSession = brandNewExecutor;
            return true;
        }
        return false;
    }

    private void EndExecutor(UpdateableChangeExecutor executor)
    {
        if (executor != currentSession)
            throw new InvalidOperationException();
        currentSession = null;
    }

    public bool TryStopActiveExecutor()
    {
        if (currentSession is null)
            return false;
        currentSession.ForceStop();
        currentSession = null;
        return true;
    }

    public void ConvertedKeyDownInlet(Key key)
    {
        currentSession?.OnConvertedKeyDown(key);
    }

    public void ConvertedKeyUpInlet(Key key)
    {
        currentSession?.OnConvertedKeyUp(key);
    }

    public void MouseMoveInlet(VecD newCanvasPos)
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

    public void OpacitySliderDragStartedInlet() => currentSession?.OnOpacitySliderDragStarted();
    public void OpacitySliderDraggedInlet(float newValue)
    {
        LastOpacityValue = newValue;
        currentSession?.OnOpacitySliderDragged(newValue);
    }
    public void OpacitySliderDragEndedInlet() => currentSession?.OnOpacitySliderDragEnded();

    public void LeftMouseButtonDownInlet(VecD canvasPos)
    {
        //update internal state
        LeftMouseState = MouseButtonState.Pressed;

        //call session event
        currentSession?.OnLeftMouseButtonDown(canvasPos);
    }

    public void LeftMouseButtonUpInlet()
    {
        //update internal state
        LeftMouseState = MouseButtonState.Released;

        //call session events
        currentSession?.OnLeftMouseButtonUp();
    }

    public void TransformMovedInlet(ShapeCorners corners)
    {
        LastTransformState = corners;
        currentSession?.OnTransformMoved(corners);
    }

    public void TransformAppliedInlet() => currentSession?.OnTransformApplied();
}
