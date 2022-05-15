using ChunkyImageLib.DataHolders;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyDocument
{
    IReadOnlyFolder ReadOnlyStructureRoot { get; }
    IReadOnlySelection ReadOnlySelection { get; }
    Vector2i Size { get; }
    bool HorizontalSymmetryEnabled { get; }
    bool VerticalSymmetryEnabled { get; }
    int HorizontalSymmetryPosition { get; }
    int VerticalSymmetryPosition { get; }
    IReadOnlyStructureMember? FindMember(Guid guid);
    IReadOnlyStructureMember FindMemberOrThrow(Guid guid);
    (IReadOnlyStructureMember, IReadOnlyFolder) FindChildAndParentOrThrow(Guid guid);
    IReadOnlyList<IReadOnlyStructureMember> FindMemberPath(Guid guid);
}
