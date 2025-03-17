using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Enums;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Views.Overlays.SymmetryOverlay;

namespace PixiEditor.Models.DocumentModels;
#nullable enable
internal class ChangeExecutionController
{
    public bool LeftMousePressed { get; private set; }
    public ShapeCorners LastTransformState { get; private set; }
    public VecI LastPixelPosition => lastPixelPos;
    public VecD LastPrecisePosition => lastPrecisePos;
    public bool IsBlockingChangeActive => currentSession is not null && currentSession.BlocksOtherActions;

    public event Action ToolSessionFinished;

    private readonly IDocument document;
    private readonly IServiceProvider services;
    private readonly DocumentInternalParts internals;

    private VecI lastPixelPos;
    private VecD lastPrecisePos;

    private UpdateableChangeExecutor? currentSession = null;

    private UpdateableChangeExecutor? _queuedExecutor = null;

    public ChangeExecutionController(IDocument document, DocumentInternalParts internals, IServiceProvider services)
    {
        this.document = document;
        this.internals = internals;
        this.services = services;
    }

    public bool IsChangeOfTypeActive<T>() where T : IExecutorFeature
    {
        return currentSession is T sessionT && sessionT.IsFeatureEnabled(sessionT);
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
        if (!CanStartExecutor(force))
            return false;
        if (force)
            currentSession?.ForceStop();

        T executor = new T();
        return TryStartExecutorInternal(executor);
    }

    public bool TryStartExecutor(UpdateableChangeExecutor brandNewExecutor, bool force = false)
    {
        if (!CanStartExecutor(force))
            return false;
        if (force)
            currentSession?.ForceStop();

        return TryStartExecutorInternal(brandNewExecutor);
    }

    private bool CanStartExecutor(bool force)
    {
        return (currentSession is null && _queuedExecutor is null) || force;
    }

    private bool TryStartExecutorInternal(UpdateableChangeExecutor executor)
    {
        executor.Initialize(document, internals, services, this, EndExecutor);

        if (executor.StartMode == ExecutorStartMode.OnMouseLeftButtonDown)
        {
            _queuedExecutor = executor;
            return true;
        }

        return StartExecutor(executor);
    }

    private bool StartExecutor(UpdateableChangeExecutor brandNewExecutor)
    {
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
        _queuedExecutor = null;

        ToolSessionFinished?.Invoke();
    }

    public bool TryStopActiveExecutor()
    {
        if (currentSession is null)
            return false;
        currentSession.ForceStop();
        currentSession = null;

        ToolSessionFinished?.Invoke();
        return true;
    }

    public void MidChangeUndoInlet()
    {
        if (currentSession is null)
            return;

        if (currentSession is IMidChangeUndoableExecutor undoableExecutor)
        {
            undoableExecutor.OnMidChangeUndo();
        }
    }

    public void MidChangeRedoInlet()
    {
        if (currentSession is null)
            return;

        if (currentSession is IMidChangeUndoableExecutor undoableExecutor)
        {
            undoableExecutor.OnMidChangeRedo();
        }
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
        currentSession?.OnOpacitySliderDragged(newValue);
    }

    public void OpacitySliderDragEndedInlet() => currentSession?.OnOpacitySliderDragEnded();

    public void SymmetryDragStartedInlet(SymmetryAxisDirection dir) => currentSession?.OnSymmetryDragStarted(dir);

    public void SymmetryDraggedInlet(SymmetryAxisDragInfo info)
    {
        currentSession?.OnSymmetryDragged(info);
    }

    public void SymmetryDragEndedInlet(SymmetryAxisDirection dir) => currentSession?.OnSymmetryDragEnded(dir);

    public void LeftMouseButtonDownInlet(MouseOnCanvasEventArgs args)
    {
        //update internal state
        LeftMousePressed = true;

        if (_queuedExecutor != null && currentSession == null)
        {
            StartExecutor(_queuedExecutor);
        }

        //call session event
        currentSession?.OnLeftMouseButtonDown(args);
    }

    public void LeftMouseButtonUpInlet(VecD argsPositionOnCanvas)
    {
        //update internal state
        LeftMousePressed = false;

        //call session events
        currentSession?.OnLeftMouseButtonUp(argsPositionOnCanvas);
    }

    public void TransformChangedInlet(ShapeCorners corners)
    {
        if (currentSession is ITransformableExecutor transformableExecutor)
        {
            LastTransformState = corners;
            transformableExecutor.OnTransformChanged(corners);
        }
    }

    public void TransformDraggedInlet(VecD from, VecD to)
    {
        if (currentSession is ITransformDraggedEvent transformableExecutor)
        {
            transformableExecutor.OnTransformDragged(from, to);
        }
    }

    public void TransformStoppedInlet()
    {
        if (currentSession is ITransformStoppedEvent transformStoppedEvent)
        {
            transformStoppedEvent.OnTransformStopped();
        }
    }

    public void MembersSelectedInlet(List<Guid> memberGuids)
    {
        currentSession?.OnMembersSelected(memberGuids);
    }

    public void TransformAppliedInlet()
    {
        if (currentSession is ITransformableExecutor transformableExecutor)
        {
            transformableExecutor.OnTransformApplied();
        }
    }

    public void SettingsChangedInlet(string name, object value)
    {
        currentSession?.OnSettingsChanged(name, value);
    }

    public void LineOverlayMovedInlet(VecD start, VecD end)
    {
        if (currentSession is ITransformableExecutor lineOverlayExecutor)
        {
            lineOverlayExecutor.OnLineOverlayMoved(start, end);
        }
    }

    public void SelectedObjectNudgedInlet(VecI distance)
    {
        if (currentSession is ITransformableExecutor transformableExecutor)
        {
            transformableExecutor.OnSelectedObjectNudged(distance);
        }
    }

    public void PrimaryColorChangedInlet(Color color)
    {
        currentSession?.OnColorChanged(color, true);
    }

    public void SecondaryColorChangedInlet(Color color)
    {
        currentSession?.OnColorChanged(color, false);
    }

    public T? TryGetExecutorFeature<T>()
    {
        if (currentSession is T feature)
            return feature;

        return default;
    }

    public void PathOverlayChangedInlet(VectorPath path)
    {
        if (currentSession is IPathExecutorFeature vectorPathToolExecutor)
        {
            vectorPathToolExecutor.OnPathChanged(path);
        }
    }

    public void TextOverlayTextChangedInlet(string text)
    {
        if (currentSession is ITextOverlayEvents textOverlayHandler)
        {
            textOverlayHandler.OnTextChanged(text);
        }
    }

    public void QuickToolSwitchInlet()
    {
        if (currentSession is IQuickToolSwitchable quickToolSwitchable)
        {
            quickToolSwitchable.OnQuickToolSwitch();
        }
    }
}
