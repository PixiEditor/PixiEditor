using PixiEditor.ChangeableDocument.Changes;

namespace PixiEditor.ChangeableDocument.Actions.Structure;

public record class MoveStructureMember_Action : IMakeChangeAction
{
    public MoveStructureMember_Action(Guid member, Guid targetFolder, int index)
    {
        Member = member;
        TargetFolder = targetFolder;
        Index = index;
    }

    public Guid Member { get; init; }
    public Guid TargetFolder { get; init; }
    public int Index { get; init; }

    Change IMakeChangeAction.CreateCorrespondingChange()
    {
        return new MoveStructureMember_Change(Member, TargetFolder, Index);
    }
}
