using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal class CreateStructureMember_Change : IChange
    {
        private Guid newMemberGuid;

        private Guid parentFolderGuid;
        private int parentFolderIndex;
        private StructureMemberType type;

        public CreateStructureMember_Change(Guid parentFolder, int parentFolderIndex, StructureMemberType type)
        {
            this.parentFolderGuid = parentFolder;
            this.parentFolderIndex = parentFolderIndex;
            this.type = type;
        }

        public void Initialize(Document target)
        {
            newMemberGuid = Guid.NewGuid();
        }

        public IChangeInfo Apply(Document document)
        {
            var folder = (Folder)document.FindMemberOrThrow(parentFolderGuid);

            StructureMember member = type switch
            {
                StructureMemberType.Layer => new Layer() { GuidValue = newMemberGuid },
                StructureMemberType.Folder => new Folder() { GuidValue = newMemberGuid },
                _ => throw new Exception("Cannon create member of type " + type.ToString())
            };

            folder.Children.Insert(parentFolderIndex, member);

            return new CreateStructureMember_ChangeInfo() { GuidValue = newMemberGuid };
        }

        public IChangeInfo Revert(Document document)
        {
            var folder = (Folder)document.FindMemberOrThrow(parentFolderGuid);
            folder.Children.RemoveAt(folder.Children.FindIndex(child => child.GuidValue == newMemberGuid));

            return new DeleteStructureMember_ChangeInfo() { GuidValue = newMemberGuid };
        }
    }
}
