using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.ChangeableDocument.Changes.Properties
{
    internal class StructureMemberName_Change : Change
    {
        private string? originalName;
        private string newName;
        private Guid targetMember;
        public StructureMemberName_Change(Guid targetMember, string newName)
        {
            this.targetMember = targetMember;
            this.newName = newName;
        }

        public override void Initialize(Document target)
        {
            var member = target.FindMemberOrThrow(targetMember);
            originalName = member.Name;
        }

        public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
        {
            if (originalName == newName)
            {
                ignoreInUndo = true;
                return null;
            }
            target.FindMemberOrThrow(targetMember).Name = newName;

            ignoreInUndo = false;
            return new StructureMemberName_ChangeInfo() { GuidValue = targetMember };
        }

        public override IChangeInfo? Revert(Document target)
        {
            if (originalName is null)
                throw new InvalidOperationException("No name to revert to");
            target.FindMemberOrThrow(targetMember).Name = originalName;
            return new StructureMemberName_ChangeInfo() { GuidValue = targetMember };
        }

        public override bool IsMergeableWith(Change other)
        {
            if (other is not StructureMemberName_Change same)
                return false;
            return same.targetMember == targetMember;
        }
    }
}
