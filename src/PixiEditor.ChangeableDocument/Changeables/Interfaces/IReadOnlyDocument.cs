using ChunkyImageLib.DataHolders;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyDocument
{
    IReadOnlyFolder ReadOnlyStructureRoot { get; }
    IReadOnlySelection ReadOnlySelection { get; }
    VecI Size { get; }
    bool HorizontalSymmetryAxisEnabled { get; }
    bool VerticalSymmetryAxisEnabled { get; }
    int HorizontalSymmetryAxisY { get; }
    int VerticalSymmetryAxisX { get; }
    IReadOnlyStructureMember? FindMember(Guid guid);
    IReadOnlyStructureMember FindMemberOrThrow(Guid guid);
    (IReadOnlyStructureMember, IReadOnlyFolder) FindChildAndParentOrThrow(Guid guid);
    IReadOnlyList<IReadOnlyStructureMember> FindMemberPath(Guid guid);
}
