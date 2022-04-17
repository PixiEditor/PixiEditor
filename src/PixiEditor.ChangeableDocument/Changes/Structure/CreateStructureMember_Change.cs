using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.ChangeableDocument.Changes.Structure
{
    internal class CreateStructureMember_Change : Change
    {
        private Guid newMemberGuid;

        private Guid parentFolderGuid;
        private int parentFolderIndex;
        private StructureMemberType type;

        public CreateStructureMember_Change(Guid parentFolder, Guid newGuid, int parentFolderIndex, StructureMemberType type)
        {
            this.parentFolderGuid = parentFolder;
            this.parentFolderIndex = parentFolderIndex;
            this.type = type;
            newMemberGuid = newGuid;
        }

        public override void Initialize(Document target) { }

        public override IChangeInfo Apply(Document document, out bool ignoreInUndo)
        {
            var folder = (Folder)document.FindMemberOrThrow(parentFolderGuid);

            StructureMember member = type switch
            {
                StructureMemberType.Layer => new Layer(document.Size) { GuidValue = newMemberGuid },
                StructureMemberType.Folder => new Folder() { GuidValue = newMemberGuid },
                _ => throw new InvalidOperationException("Cannon create member of type " + type.ToString())
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
