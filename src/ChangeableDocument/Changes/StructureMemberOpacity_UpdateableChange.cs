using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal class StructureMemberOpacity_UpdateableChange : IUpdateableChange
    {
        private Guid memberGuid;

        private float originalOpacity;
        private float newOpacity;

        public StructureMemberOpacity_UpdateableChange(Guid memberGuid, float opacity)
        {
            this.memberGuid = memberGuid;
            newOpacity = opacity;
        }

        public void Update(float updatedOpacity)
        {
            newOpacity = updatedOpacity;
        }

        public void Initialize(Document document)
        {
            var member = document.FindMemberOrThrow(memberGuid);
            originalOpacity = member.Opacity;
        }

        public IChangeInfo? ApplyTemporarily(Document target) => Apply(target, out _);

        public IChangeInfo? Apply(Document document, out bool ignoreInUndo)
        {
            if (originalOpacity == newOpacity)
            {
                ignoreInUndo = true;
                return null;
            }

            var member = document.FindMemberOrThrow(memberGuid);
            member.Opacity = newOpacity;

            ignoreInUndo = false;
            return new StructureMemberOpacity_ChangeInfo() { GuidValue = memberGuid };
        }

        public IChangeInfo? Revert(Document document)
        {
            if (originalOpacity == newOpacity)
                return null;

            var member = document.FindMemberOrThrow(memberGuid);
            member.Opacity = originalOpacity;

            return new StructureMemberOpacity_ChangeInfo() { GuidValue = memberGuid };
        }

        public void Dispose() { }
    }
}
