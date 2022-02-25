using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal class Document_DeleteStructureMember_Change : Change<Document>
    {
        private Guid memberGuid;
        private Guid parentGuid;
        private int originalIndex;
        private StructureMember? savedCopy;
        public Document_DeleteStructureMember_Change(Guid memberGuid)
        {
            this.memberGuid = memberGuid;
        }

        protected override void DoInitialize(Document document)
        {
            var (member, parent) = document.FindChildAndParentOrThrow(memberGuid);

            originalIndex = parent.Children.IndexOf(member);
            parentGuid = parent.GuidValue;
            savedCopy = member.Clone();
        }

        protected override IChangeInfo DoApply(Document document)
        {
            var (member, parent) = document.FindChildAndParentOrThrow(memberGuid);
            parent.Children.Remove(member);
            return new Document_DeleteStructureMember_ChangeInfo() { GuidValue = memberGuid };
        }

        protected override IChangeInfo DoRevert(Document doc)
        {
            var parent = (Folder)doc.FindMemberOrThrow(parentGuid);

            parent.Children.Insert(originalIndex, savedCopy!.Clone());
            return new Document_CreateStructureMember_ChangeInfo() { GuidValue = memberGuid };
        }
    }
}
