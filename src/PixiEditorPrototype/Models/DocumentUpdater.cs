using ChangeableDocument.Changeables.Interfaces;
using ChangeableDocument.ChangeInfos;
using PixiEditorPrototype.ViewModels;
using System;

namespace PixiEditorPrototype.Models
{
    internal class DocumentUpdater
    {
        private DocumentViewModel doc;
        public DocumentUpdater(DocumentViewModel doc)
        {
            this.doc = doc;
        }

        public void ApplyChangeFromChangeInfo(IChangeInfo? arbitraryInfo)
        {
            if (arbitraryInfo == null)
                return;

            switch (arbitraryInfo)
            {
                case Document_CreateStructureMember_ChangeInfo info:
                    ProcessCreateStructureMember(info);
                    break;
                case Document_DeleteStructureMember_ChangeInfo info:
                    ProcessDeleteStructureMember(info);
                    break;
                case Document_UpdateStructureMemberProperties_ChangeInfo info:
                    ProcessUpdateStructureMemberProperties(info);
                    break;
                case Document_MoveStructureMember_ChangeInfo info:
                    ProcessMoveStructureMember(info);
                    break;
            }
        }

        private void ProcessCreateStructureMember(Document_CreateStructureMember_ChangeInfo info)
        {
            var (member, parentFolder) = doc.Tracker.Document.FindChildAndParentOrThrow(info.GuidValue);
            var parentFolderVM = (FolderViewModel)doc.StructureHelper.FindOrThrow(parentFolder.GuidValue);

            int index = parentFolder.ReadOnlyChildren.IndexOf(member);

            StructureMemberViewModel memberVM = member switch
            {
                IReadOnlyLayer layer => new LayerViewModel(doc, layer),
                IReadOnlyFolder folder => new FolderViewModel(doc, folder),
                _ => throw new Exception("Unsupposed member type")
            };

            parentFolderVM.Children.Insert(index, memberVM);

            if (member is IReadOnlyFolder folder2)
            {
                foreach (IReadOnlyStructureMember child in folder2.ReadOnlyChildren)
                {
                    ProcessCreateStructureMember(new Document_CreateStructureMember_ChangeInfo() { GuidValue = child.GuidValue });
                }
            }
        }

        private void ProcessDeleteStructureMember(Document_DeleteStructureMember_ChangeInfo info)
        {
            var (memberVM, folderVM) = doc.StructureHelper.FindChildAndParentOrThrow(info.GuidValue);
            folderVM.Children.Remove(memberVM);
        }

        private void ProcessUpdateStructureMemberProperties(Document_UpdateStructureMemberProperties_ChangeInfo info)
        {
            var memberVM = doc.StructureHelper.FindOrThrow(info.GuidValue);
            if (info.NameChanged) memberVM.RaisePropertyChanged(nameof(memberVM.Name));
            if (info.IsVisibleChanged) memberVM.RaisePropertyChanged(nameof(memberVM.IsVisible));
        }

        private void ProcessMoveStructureMember(Document_MoveStructureMember_ChangeInfo info)
        {
            var (memberVM, curFolderVM) = doc.StructureHelper.FindChildAndParentOrThrow(info.GuidValue);
            var (member, targetFolder) = doc.Tracker.Document.FindChildAndParentOrThrow(info.GuidValue);

            int index = targetFolder.ReadOnlyChildren.IndexOf(member);
            var targetFolderVM = (FolderViewModel)doc.StructureHelper.FindOrThrow(targetFolder.GuidValue);

            curFolderVM.Children.Remove(memberVM);
            targetFolderVM.Children.Insert(index, memberVM);
        }
    }
}
