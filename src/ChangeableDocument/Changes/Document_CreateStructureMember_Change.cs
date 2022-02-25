using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal class Document_CreateStructureMember_Change : Change<Document>
    {
        private Guid newMemberGuid;

        private Guid parentFolderGuid;
        private int parentFolderIndex;
        private StructureMemberType type;

        public Document_CreateStructureMember_Change(Guid parentFolder, int parentFolderIndex, StructureMemberType type)
        {
            this.parentFolderGuid = parentFolder;
            this.parentFolderIndex = parentFolderIndex;
            this.type = type;
        }

        protected override void DoInitialize(Document target)
        {
            newMemberGuid = Guid.NewGuid();
        }

        protected override IChangeInfo DoApply(Document document)
        {
            var folder = (Folder)document.FindMemberOrThrow(parentFolderGuid);

            StructureMember member = type switch
            {
                StructureMemberType.Layer => new Layer() { GuidValue = newMemberGuid },
                StructureMemberType.Folder => new Folder() { GuidValue = newMemberGuid },
                _ => throw new Exception("Cannon create member of type " + type.ToString())
            };

            folder.Children.Insert(parentFolderIndex, member);

            return new Document_CreateStructureMember_ChangeInfo() { GuidValue = newMemberGuid };
        }

        protected override IChangeInfo DoRevert(Document document)
        {
            var folder = (Folder)document.FindMemberOrThrow(parentFolderGuid);
            folder.Children.RemoveAt(folder.Children.FindIndex(child => child.GuidValue == newMemberGuid));

            return new Document_DeleteStructureMember_ChangeInfo() { GuidValue = newMemberGuid };
        }
    }
}
