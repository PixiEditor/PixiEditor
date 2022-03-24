using ChangeableDocument.Changes;
using ChangeableDocument.Changes.Drawing;

namespace ChangeableDocument.Actions.Drawing
{
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
}
