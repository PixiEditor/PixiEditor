namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyFolder : IReadOnlyStructureMember
{
    /// <summary>
    /// The children of the folder
    /// </summary>
    IReadOnlyList<IReadOnlyStructureMember> Children { get; }
}
