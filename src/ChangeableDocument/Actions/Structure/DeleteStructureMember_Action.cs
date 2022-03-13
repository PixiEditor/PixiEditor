using ChangeableDocument.Changes;

namespace ChangeableDocument.Actions.Structure;

public record struct DeleteStructureMember_Action : IMakeChangeAction
{
    public DeleteStructureMember_Action(Guid guidValue)
    {
        GuidValue = guidValue;
    }

    public Guid GuidValue { get; }

    IChange IMakeChangeAction.CreateCorrespondingChange()
    {
        return new DeleteStructureMember_Change(GuidValue);
    }
}
