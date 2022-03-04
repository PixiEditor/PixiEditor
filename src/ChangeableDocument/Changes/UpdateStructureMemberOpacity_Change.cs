using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal class UpdateStructureMemberOpacity_Change : IUpdateableChange
    {
        private Guid memberGuid;

        private float originalOpacity;
        public float NewOpacity { get; private set; }

        public UpdateStructureMemberOpacity_Change(Guid memberGuid, float opacity)
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
            originalOpacity = member.Opacity;
        }

        public IChangeInfo? Apply(Document document)
        {
            var member = document.FindMemberOrThrow(memberGuid);
            member.Opacity = NewOpacity;

            return new StructureMemberOpacity_ChangeInfo() { GuidValue = memberGuid };
        }

        public IChangeInfo? Revert(Document document)
        {
            var member = document.FindMemberOrThrow(memberGuid);
            member.Opacity = originalOpacity;

            return new StructureMemberOpacity_ChangeInfo() { GuidValue = memberGuid };
        }
    }
}
