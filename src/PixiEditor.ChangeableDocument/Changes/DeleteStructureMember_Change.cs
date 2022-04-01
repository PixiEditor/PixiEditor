using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.ChangeableDocument.Changes
{
    internal class DeleteStructureMember_Change : Change
    {
        private Guid memberGuid;
        private Guid parentGuid;
        private int originalIndex;
        private StructureMember? savedCopy;
        public DeleteStructureMember_Change(Guid memberGuid)
        {
            this.memberGuid = memberGuid;
        }

        public override void Initialize(Document document)
        {
            var (member, parent) = document.FindChildAndParentOrThrow(memberGuid);

            originalIndex = parent.Children.IndexOf(member);
            parentGuid = parent.GuidValue;
            savedCopy = member.Clone();
        }

        public override IChangeInfo Apply(Document document, out bool ignoreInUndo)
        {
            var (member, parent) = document.FindChildAndParentOrThrow(memberGuid);
            parent.Children.Remove(member);
            member.Dispose();
            ignoreInUndo = false;
            return new DeleteStructureMember_ChangeInfo() { GuidValue = memberGuid };
        }

        public override IChangeInfo Revert(Document doc)
        {
            var parent = (Folder)doc.FindMemberOrThrow(parentGuid);

            parent.Children.Insert(originalIndex, savedCopy!.Clone());
            return new CreateStructureMember_ChangeInfo() { GuidValue = memberGuid };
        }

        public override void Dispose()
        {
            savedCopy!.Dispose();
        }
    }
}
