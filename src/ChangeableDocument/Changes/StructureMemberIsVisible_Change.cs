using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal class StructureMemberIsVisible_Change : Change
    {
        private bool? originalIsVisible;
        private bool newIsVisible;
        private Guid targetMember;
        public StructureMemberIsVisible_Change(Guid targetMember, bool newIsVisible)
        {
            this.targetMember = targetMember;
            this.newIsVisible = newIsVisible;
        }

        public override void Initialize(Document target)
        {
            var member = target.FindMemberOrThrow(targetMember);
            originalIsVisible = member.IsVisible;
        }

        public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
        {
            // don't record layer/folder visibility changes - it's just more convenient this way
            ignoreInUndo = true;
            if (originalIsVisible == newIsVisible)
                return null;
            target.FindMemberOrThrow(targetMember).IsVisible = newIsVisible;

            return new StructureMemberIsVisible_ChangeInfo() { GuidValue = targetMember };
        }

        public override IChangeInfo? Revert(Document target)
        {
            target.FindMemberOrThrow(targetMember).IsVisible = originalIsVisible!.Value;
            return new StructureMemberIsVisible_ChangeInfo() { GuidValue = targetMember };
        }
    }
}
