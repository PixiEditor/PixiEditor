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
                case CreateStructureMember_ChangeInfo info:
                    ProcessCreateStructureMember(info);
                    break;
                case DeleteStructureMember_ChangeInfo info:
                    ProcessDeleteStructureMember(info);
                    break;
                case StructureMemberProperties_ChangeInfo info:
                    ProcessUpdateStructureMemberProperties(info);
                    break;
                case StructureMemberOpacity_ChangeInfo info:
                    ProcessUpdateStructureMemberOpacity(info);
                    break;
                case MoveStructureMember_ChangeInfo info:
                    ProcessMoveStructureMember(info);
                    break;
            }
        }

        private void ProcessCreateStructureMember(CreateStructureMember_ChangeInfo info)
        {
            var (member, parentFolder) = doc.Tracker.Document.FindChildAndParentOrThrow(info.GuidValue);
            var parentFolderVM = (FolderViewModel)doc.StructureHelper.FindOrThrow(parentFolder.ReadOnlyGuidValue);

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
                    ProcessCreateStructureMember(new CreateStructureMember_ChangeInfo() { GuidValue = child.ReadOnlyGuidValue });
                }
            }
        }

        private void ProcessDeleteStructureMember(DeleteStructureMember_ChangeInfo info)
        {
            var (memberVM, folderVM) = doc.StructureHelper.FindChildAndParentOrThrow(info.GuidValue);
            folderVM.Children.Remove(memberVM);
        }

        private void ProcessUpdateStructureMemberProperties(StructureMemberProperties_ChangeInfo info)
        {
            var memberVM = doc.StructureHelper.FindOrThrow(info.GuidValue);
            if (info.NameChanged) memberVM.RaisePropertyChanged(nameof(memberVM.Name));
            if (info.IsVisibleChanged) memberVM.RaisePropertyChanged(nameof(memberVM.IsVisible));
        }

        private void ProcessUpdateStructureMemberOpacity(StructureMemberOpacity_ChangeInfo info)
        {
            var memberVM = doc.StructureHelper.FindOrThrow(info.GuidValue);
            memberVM.RaisePropertyChanged(nameof(memberVM.Opacity));
        }

        private void ProcessMoveStructureMember(MoveStructureMember_ChangeInfo info)
        {
            var (memberVM, curFolderVM) = doc.StructureHelper.FindChildAndParentOrThrow(info.GuidValue);
            var (member, targetFolder) = doc.Tracker.Document.FindChildAndParentOrThrow(info.GuidValue);

            int index = targetFolder.ReadOnlyChildren.IndexOf(member);
            var targetFolderVM = (FolderViewModel)doc.StructureHelper.FindOrThrow(targetFolder.ReadOnlyGuidValue);

            curFolderVM.Children.Remove(memberVM);
            targetFolderVM.Children.Insert(index, memberVM);
        }
    }
}
