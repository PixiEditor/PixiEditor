using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Drawing;

namespace PixiEditor.ChangeableDocument.Actions.Drawing;

public record class CombineStructureMembersOnto_Action : IMakeChangeAction
{
    public CombineStructureMembersOnto_Action(Guid targetLayer, HashSet<Guid> membersToCombine)
    {
        TargetLayer = targetLayer;
        MembersToCombine = membersToCombine;
    }

    public Guid TargetLayer { get; }
    public HashSet<Guid> MembersToCombine { get; }
    Change IMakeChangeAction.CreateCorrespondingChange()
    {
        return new CombineStructureMembersOnto_Change(MembersToCombine, TargetLayer);
    }
}
