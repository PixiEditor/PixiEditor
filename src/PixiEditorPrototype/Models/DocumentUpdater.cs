using ChangeableDocument.Changeables.Interfaces;
using ChangeableDocument.ChangeInfos;
using PixiEditorPrototype.ViewModels;
using SkiaSharp;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
                case StructureMemberName_ChangeInfo info:
                    ProcessUpdateStructureMemberName(info);
                    break;
                case StructureMemberIsVisible_ChangeInfo info:
                    ProcessUpdateStructureMemberIsVisible(info);
                    break;
                case StructureMemberOpacity_ChangeInfo info:
                    ProcessUpdateStructureMemberOpacity(info);
                    break;
                case MoveStructureMember_ChangeInfo info:
                    ProcessMoveStructureMember(info);
                    break;
                case Size_ChangeInfo info:
                    ProcessSize(info);
                    break;
            }
        }

        private void ProcessSize(Size_ChangeInfo info)
        {
            doc.FinalBitmapSurface.Dispose();

            doc.FinalBitmap = new WriteableBitmap(doc.Tracker.Document.Size.X, doc.Tracker.Document.Size.Y, 96, 96, PixelFormats.Pbgra32, null);
            doc.FinalBitmapSurface = SKSurface.Create(
                new SKImageInfo(doc.FinalBitmap.PixelWidth, doc.FinalBitmap.PixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()),
                doc.FinalBitmap.BackBuffer,
                doc.FinalBitmap.BackBufferStride);
        }

        private void ProcessCreateStructureMember(CreateStructureMember_ChangeInfo info)
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
                    ProcessCreateStructureMember(new CreateStructureMember_ChangeInfo() { GuidValue = child.GuidValue });
                }
            }
        }

        private void ProcessDeleteStructureMember(DeleteStructureMember_ChangeInfo info)
        {
            var (memberVM, folderVM) = doc.StructureHelper.FindChildAndParentOrThrow(info.GuidValue);
            folderVM.Children.Remove(memberVM);
        }

        private void ProcessUpdateStructureMemberIsVisible(StructureMemberIsVisible_ChangeInfo info)
        {
            var memberVM = doc.StructureHelper.FindOrThrow(info.GuidValue);
            memberVM.RaisePropertyChanged(nameof(memberVM.IsVisible));
        }

        private void ProcessUpdateStructureMemberName(StructureMemberName_ChangeInfo info)
        {
            var memberVM = doc.StructureHelper.FindOrThrow(info.GuidValue);
            memberVM.RaisePropertyChanged(nameof(memberVM.Name));
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
            var targetFolderVM = (FolderViewModel)doc.StructureHelper.FindOrThrow(targetFolder.GuidValue);

            curFolderVM.Children.Remove(memberVM);
            targetFolderVM.Children.Insert(index, memberVM);
        }
    }
}
