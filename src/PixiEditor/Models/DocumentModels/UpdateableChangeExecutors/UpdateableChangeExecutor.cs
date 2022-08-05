using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal abstract class UpdateableChangeExecutor
{
    protected DocumentViewModel? document;
    protected DocumentInternalParts? internals;
    protected ChangeExecutionController? controller;
    private bool initialized = false;

    protected Action<UpdateableChangeExecutor>? onEnded;

    public void Initialize(DocumentViewModel document, DocumentInternalParts internals, ChangeExecutionController controller, Action<UpdateableChangeExecutor> onEnded)
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
    public virtual void OnConvertedKeyDown(Key key) { }
    public virtual void OnConvertedKeyUp(Key key) { }
    public virtual void OnTransformMoved(ShapeCorners corners) { }
    public virtual void OnTransformApplied() { }
}
