using System.Collections.Generic;
using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.Exceptions;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Position;
using PixiEditor.AvaloniaUI.Models.Tools;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal abstract class UpdateableChangeExecutor
{
    protected IDocument? document;
    protected DocumentInternalParts? internals;
    protected ChangeExecutionController? controller;
    protected IReadOnlyCollection<IHandler> handlers;
    private bool initialized = false;

    protected Action<UpdateableChangeExecutor>? onEnded;
    public virtual ExecutorType Type => ExecutorType.Regular;
    public virtual ExecutorStartMode StartMode => ExecutorStartMode.RightAway;

    public void Initialize(IDocument document, DocumentInternalParts internals, List<IHandler> list,
        ChangeExecutionController controller, Action<UpdateableChangeExecutor> onEnded)
    {
        if (initialized)
            throw new InvalidOperationException();
        initialized = true;

        this.document = document;
        this.internals = internals;
        this.controller = controller;
        handlers = list;
        this.onEnded = onEnded;
    }

    protected T GetHandler<T>()
        where T : IHandler
    {
        foreach (var handler in handlers)
        {
            if (handler is T t)
            {
                return t;
            }
        }

        throw new DependencyNotFoundException(typeof(T));
    }

    public abstract ExecutionState Start();
    public abstract void ForceStop();
    public virtual void OnPixelPositionChange(VecI pos) { }
    public virtual void OnPrecisePositionChange(VecD pos) { }
    public virtual void OnLeftMouseButtonDown(VecD pos) { }
    public virtual void OnLeftMouseButtonUp() { }
    public virtual void OnOpacitySliderDragStarted() { }
    public virtual void OnOpacitySliderDragged(float newValue) { }
    public virtual void OnOpacitySliderDragEnded() { }
    public virtual void OnSymmetryDragStarted(SymmetryAxisDirection dir) { }
    public virtual void OnSymmetryDragged(SymmetryAxisDragInfo info) { }
    public virtual void OnSymmetryDragEnded(SymmetryAxisDirection dir) { }
    public virtual void OnConvertedKeyDown(Key key) { }
    public virtual void OnConvertedKeyUp(Key key) { }
    public virtual void OnTransformMoved(ShapeCorners corners) { }
    public virtual void OnTransformApplied() { }
    public virtual void OnLineOverlayMoved(VecD start, VecD end) { }
    public virtual void OnMidChangeUndo() { }
    public virtual void OnMidChangeRedo() { }
    public virtual void OnSelectedObjectNudged(VecI distance) { }
}
