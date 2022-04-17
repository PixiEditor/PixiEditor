using ChunkyImageLib;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.ChangeableDocument.Changes.Properties
{
    internal class DeleteStructureMemberMask_Change : Change
    {
        private readonly Guid memberGuid;
        private ChunkyImage? storedMask;

        public DeleteStructureMemberMask_Change(Guid memberGuid)
        {
            this.memberGuid = memberGuid;
        }

        public override void Initialize(Document target)
        {
            var member = target.FindMemberOrThrow(memberGuid);
            if (member.Mask is null)
                throw new InvalidOperationException("Cannot delete the mask; Target member has no mask");
            storedMask = member.Mask.CloneFromLatest();
        }

        public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
        {
            var member = target.FindMemberOrThrow(memberGuid);
            if (member.Mask is null)
                throw new InvalidOperationException("Cannot delete the mask; Target member has no mask");
            member.Mask.Dispose();
            member.Mask = null;

            ignoreInUndo = false;
            return new StructureMemberMask_ChangeInfo() { GuidValue = memberGuid };
        }

        public override IChangeInfo? Revert(Document target)
        {
            var member = target.FindMemberOrThrow(memberGuid);
            if (member.Mask is not null)
                throw new InvalidOperationException("Cannot revert mask deletion; The target member already has a mask");
            member.Mask = storedMask!.CloneFromLatest();

            return new StructureMemberMask_ChangeInfo() { GuidValue = memberGuid };
        }

        public override void Dispose()
        {
            storedMask?.Dispose();
        }
    }
}
