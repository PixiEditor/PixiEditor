using ChangeableDocument.Changes;

namespace ChangeableDocument.Actions.Structure;

public record class CreateStructureMember_Action : IMakeChangeAction
{
    public CreateStructureMember_Action(Guid parentGuid, Guid newGuid, int index, StructureMemberType type)
    {
        ParentGuid = parentGuid;
        NewGuid = newGuid;
        Index = index;
        Type = type;
    }

    public Guid ParentGuid { get; init; }
    public Guid NewGuid { get; init; }
    public int Index { get; init; }
    public StructureMemberType Type { get; init; }

    Change IMakeChangeAction.CreateCorrespondingChange()
    {
        return new CreateStructureMember_Change(ParentGuid, NewGuid, Index, Type);
    }
}
