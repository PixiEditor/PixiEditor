using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.ChangeableDocument.Changes.Structure
{
    internal class MoveStructureMember_Change : Change
    {
        private Guid memberGuid;

        private Guid targetFolderGuid;
        private int targetFolderIndex;

        private Guid originalFolderGuid;
        private int originalFolderIndex;

        public MoveStructureMember_Change(Guid memberGuid, Guid targetFolder, int targetFolderIndex)
        {
            this.memberGuid = memberGuid;
            this.targetFolderGuid = targetFolder;
            this.targetFolderIndex = targetFolderIndex;
        }

        public override void Initialize(Document document)
        {
            var (member, curFolder) = document.FindChildAndParentOrThrow(memberGuid);
            originalFolderGuid = curFolder.GuidValue;
            originalFolderIndex = curFolder.Children.IndexOf(member);
        }

        private static void Move(Document document, Guid memberGuid, Guid targetFolderGuid, int targetIndex)
        {
            var targetFolder = (Folder)document.FindMemberOrThrow(targetFolderGuid);
            var (member, curFolder) = document.FindChildAndParentOrThrow(memberGuid);

            curFolder.Children.Remove(member);
            targetFolder.Children.Insert(targetIndex, member);
        }

        public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
        {
            Move(target, memberGuid, targetFolderGuid, targetFolderIndex);
            ignoreInUndo = false;
            return new MoveStructureMember_ChangeInfo() { GuidValue = memberGuid };
        }

        public override IChangeInfo? Revert(Document target)
        {
            Move(target, memberGuid, originalFolderGuid, originalFolderIndex);
            return new MoveStructureMember_ChangeInfo() { GuidValue = memberGuid };
        }
    }
}
