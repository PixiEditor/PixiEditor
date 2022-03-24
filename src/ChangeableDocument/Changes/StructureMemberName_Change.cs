using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal class StructureMemberName_Change : IChange
    {
        private string? originalName;
        private string newName;
        private Guid targetMember;
        public StructureMemberName_Change(Guid targetMember, string newName)
        {
            this.targetMember = targetMember;
            this.newName = newName;
        }

        public void Initialize(Document target)
        {
            var member = target.FindMemberOrThrow(targetMember);
            originalName = member.Name;
        }

        public IChangeInfo? Apply(Document target, out bool ignoreInUndo)
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

        public IChangeInfo? Revert(Document target)
        {
            if (originalName == null)
                throw new Exception("No name to revert to");
            target.FindMemberOrThrow(targetMember).Name = originalName;
            return new StructureMemberName_ChangeInfo() { GuidValue = targetMember };
        }

        public void Dispose() { }
    }
}
