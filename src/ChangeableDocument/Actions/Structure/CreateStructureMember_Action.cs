using ChangeableDocument.Changes;

namespace ChangeableDocument.Actions.Structure;

public record class CreateStructureMember_Action : IMakeChangeAction
{
    public CreateStructureMember_Action(Guid parentGuid, int index, StructureMemberType type)
    {
        ParentGuid = parentGuid;
        Index = index;
        Type = type;
    }

    public Guid ParentGuid { get; init; }
    public int Index { get; init; }
    public StructureMemberType Type { get; init; }

    IChange IMakeChangeAction.CreateCorrespondingChange()
    {
        return new CreateStructureMember_Change(ParentGuid, Index, Type);
    }
}
