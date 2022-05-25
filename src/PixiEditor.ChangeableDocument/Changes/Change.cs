using OneOf;
using OneOf.Types;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.ChangeableDocument.Changes;

internal abstract class Change : IDisposable
{
    public virtual bool IsMergeableWith(Change other) => false;
    public abstract OneOf<Success, Error> InitializeAndValidate(Document target);
    public abstract IChangeInfo? Apply(Document target, out bool ignoreInUndo);
    public abstract IChangeInfo? Revert(Document target);
    public virtual void Dispose() { }
};
