namespace PixiEditor.ChangeableDocument.Changes;

internal abstract class Change : IDisposable
{
    public virtual bool IsMergeableWith(Change other) => false;
    public abstract OneOf<Success, Error> InitializeAndValidate(Document target);
    public abstract OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo);
    public abstract OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target);
    public virtual void Dispose() { }
};
