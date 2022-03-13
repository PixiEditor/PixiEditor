using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal class StructureMemberOpacity_UpdateableChange : IUpdateableChange
    {
        private Guid memberGuid;

        private float originalOpacity;
        public float NewOpacity { get; private set; }

        public StructureMemberOpacity_UpdateableChange(Guid memberGuid, float opacity)
        {
            this.memberGuid = memberGuid;
            NewOpacity = opacity;
        }

        public void Update(float updatedOpacity)
        {
            NewOpacity = updatedOpacity;
        }

        public void Initialize(Document document)
        {
            var member = document.FindMemberOrThrow(memberGuid);
            originalOpacity = member.ReadOnlyOpacity;
        }

        public IChangeInfo? ApplyTemporarily(Document target) => Apply(target);

        public IChangeInfo? Apply(Document document)
        {
            var member = document.FindMemberOrThrow(memberGuid);
            member.ReadOnlyOpacity = NewOpacity;

            return new StructureMemberOpacity_ChangeInfo() { GuidValue = memberGuid };
        }

        public IChangeInfo? Revert(Document document)
        {
            var member = document.FindMemberOrThrow(memberGuid);
            member.ReadOnlyOpacity = originalOpacity;

            return new StructureMemberOpacity_ChangeInfo() { GuidValue = memberGuid };
        }


    }
}
