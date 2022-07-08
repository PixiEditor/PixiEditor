using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal abstract class UpdateableChangeExecutor
{
    protected DocumentViewModel? document;
    protected DocumentHelpers? helpers;
    protected ChangeExecutionController? controller;
    private bool initialized = false;

    protected Action<UpdateableChangeExecutor>? onEnded;

    public void Initialize(DocumentViewModel document, DocumentHelpers helpers, ChangeExecutionController controller, Action<UpdateableChangeExecutor> onEnded)
    {
        if (initialized)
            throw new InvalidOperationException();
        initialized = true;

        this.document = document;
        this.helpers = helpers;
        this.controller = controller;
        this.onEnded = onEnded;
    }

    public abstract OneOf<Success, Error> Start();
    public abstract void ForceStop();
    public virtual void OnPixelPositionChange(VecI pos) { }
    public virtual void OnPrecisePositionChange(VecD pos) { }
    public virtual void OnLeftMouseButtonDown(VecD pos) { }
    public virtual void OnLeftMouseButtonUp() { }
    public virtual void OnKeyDown(Key key) { }
    public virtual void OnKeyUp(Key key) { }
}
