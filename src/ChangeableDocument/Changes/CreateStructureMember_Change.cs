using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal class CreateStructureMember_Change : Change
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

        public override void Initialize(Document target)
        {
            newMemberGuid = Guid.NewGuid();
        }

        public override IChangeInfo Apply(Document document, out bool ignoreInUndo)
        {
            var folder = (Folder)document.FindMemberOrThrow(parentFolderGuid);

            StructureMember member = type switch
            {
                StructureMemberType.Layer => new Layer(document.Size) { GuidValue = newMemberGuid },
                StructureMemberType.Folder => new Folder() { GuidValue = newMemberGuid },
                _ => throw new Exception("Cannon create member of type " + type.ToString())
            };

            folder.Children.Insert(parentFolderIndex, member);

            ignoreInUndo = false;
            return new CreateStructureMember_ChangeInfo() { GuidValue = newMemberGuid };
        }

        public override IChangeInfo Revert(Document document)
        {
            var folder = (Folder)document.FindMemberOrThrow(parentFolderGuid);
            var child = document.FindMemberOrThrow(newMemberGuid);
            child.Dispose();
            folder.Children.RemoveAt(folder.Children.FindIndex(child => child.GuidValue == newMemberGuid));

            return new DeleteStructureMember_ChangeInfo() { GuidValue = newMemberGuid };
        }
    }
}
