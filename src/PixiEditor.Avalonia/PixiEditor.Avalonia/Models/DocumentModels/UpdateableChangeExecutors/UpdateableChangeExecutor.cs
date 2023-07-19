using System.Windows.Input;
using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Containers;
using PixiEditor.Models.Enums;
using PixiEditor.Views.UserControls.SymmetryOverlay;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal abstract class UpdateableChangeExecutor
{
    protected IDocument? document;
    protected DocumentInternalParts? internals;
    protected ChangeExecutionController? controller;
    private bool initialized = false;

    protected Action<UpdateableChangeExecutor>? onEnded;
    public virtual ExecutorType Type => ExecutorType.Regular;
    public virtual ExecutorStartMode StartMode => ExecutorStartMode.RightAway;

    public void Initialize(IDocument document, DocumentInternalParts internals, ChangeExecutionController controller, Action<UpdateableChangeExecutor> onEnded)
    {
        if (initialized)
            throw new InvalidOperationException();
        initialized = true;

        this.document = document;
        this.internals = internals;
        this.controller = controller;
        this.onEnded = onEnded;
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
