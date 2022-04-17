using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Structure;

namespace PixiEditor.ChangeableDocument.Actions.Structure;

public record class DeleteStructureMember_Action : IMakeChangeAction
{
    public DeleteStructureMember_Action(Guid guidValue)
    {
        GuidValue = guidValue;
    }

    public Guid GuidValue { get; }

    Change IMakeChangeAction.CreateCorrespondingChange()
    {
        return new DeleteStructureMember_Change(GuidValue);
    }
}
