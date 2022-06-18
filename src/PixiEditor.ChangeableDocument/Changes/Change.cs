namespace PixiEditor.ChangeableDocument.Changes;

internal abstract class Change : IDisposable
{
    /// <summary>
    /// Check's if the layer is mergeable with the <paramref name="other"/> change. Returns false if not overridden
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool IsMergeableWith(Change other) => false;
    /// <summary>
    /// Initializes and validates the changes parameter. Called before Apply and ApplyTemporarily
    /// </summary>
    /// <param name="target">The document the change will be applied on</param>
    /// <returns><see cref="Success"/> if all parameters are valid, otherwise <see cref="Error"/></returns>
    public abstract OneOf<Success, Error> InitializeAndValidate(Document target);
    /// <summary>
    /// Applies the change to the <paramref name="target"/>
    /// </summary>
    /// <param name="firstApply">True if this is the first time Apply was called, always true if ignoreInUndo is true</param>
    /// <param name="ignoreInUndo">Should this change be undoable using <see cref="Revert"/></param>
    /// <returns>Either nothing, a single change info, or a collection of change infos</returns>
    public abstract OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo);
    /// <summary>
    /// Reverts the <paramref name="target"/> to the state before <see cref="Apply"/> was called. Not executed if ignoreInUndo was set in <see cref="Apply"/>
    /// </summary>
    /// <returns>Either nothing, a single change info, or a collection of change infos</returns>
    public abstract OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target);
    public virtual void Dispose() { }
};
