using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal class StructureMemberVisibility_Change : IChange
    {
        private bool? originalIsVisible;
        private bool newIsVisible;
        private Guid targetMember;
        public StructureMemberVisibility_Change(Guid targetMember, bool newIsVisible)
        {
            this.targetMember = targetMember;
            this.newIsVisible = newIsVisible;
        }

        public void Initialize(Document target)
        {
            var member = target.FindMemberOrThrow(targetMember);
            originalIsVisible = member.IsVisible;
        }

        public IChangeInfo? Apply(Document target, out bool ignoreInUndo)
        {
            // don't record layer/folder visibility changes - it's just more convenient this way
            ignoreInUndo = true;
            if (originalIsVisible == newIsVisible)
                return null;
            target.FindMemberOrThrow(targetMember).IsVisible = newIsVisible;

            return new StructureMemberIsVisible_ChangeInfo() { GuidValue = targetMember };
        }

        public IChangeInfo? Revert(Document target)
        {
            if (originalIsVisible == null)
                throw new Exception("No name to revert to");
            target.FindMemberOrThrow(targetMember).IsVisible = originalIsVisible.Value;
            return new StructureMemberIsVisible_ChangeInfo() { GuidValue = targetMember };
        }

        public void Dispose() { }
    }
}
