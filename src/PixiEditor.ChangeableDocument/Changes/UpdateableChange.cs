namespace PixiEditor.ChangeableDocument.Changes;

internal abstract class UpdateableChange : Change
{
    /// <summary>
    /// Applies the change temporarily
    /// </summary>
    /// <param name="target">The document the change will be applied on</param>
    /// <returns>Either nothing, a single change info, or a collection of change infos</returns>
    public abstract OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target);
}
