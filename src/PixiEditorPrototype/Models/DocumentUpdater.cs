using ChangeableDocument.Changeables.Interfaces;
using ChangeableDocument.ChangeInfos;
using ChunkyImageLib.DataHolders;
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
            if (arbitraryInfo is null)
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
                case MoveViewport_PassthroughAction info:
                    ProcessMoveViewport(info);
                    break;
            }
        }

        private void ProcessMoveViewport(MoveViewport_PassthroughAction info)
        {
            doc.ChosenResolution = info.Resolution;
            doc.RaisePropertyChanged(nameof(doc.RenderBitmap));
        }

        private void ProcessSize(Size_ChangeInfo info)
        {
            doc.SurfaceFull.Dispose();
            doc.SurfaceHalf?.Dispose();
            doc.SurfaceQuarter?.Dispose();
            doc.SurfaceEighth?.Dispose();

            doc.SurfaceHalf = null;
            doc.SurfaceQuarter = null;
            doc.SurfaceEighth = null;

            doc.BitmapHalf = null;
            doc.BitmapQuarter = null;
            doc.BitmapEighth = null;

            doc.BitmapFull = CreateBitmap(doc.Tracker.Document.Size);
            doc.SurfaceFull = CreateSKSurface(doc.BitmapFull);

            if (doc.Tracker.Document.Size.X > 512 && doc.Tracker.Document.Size.Y > 512)
            {
                doc.BitmapHalf = CreateBitmap(doc.Tracker.Document.Size / 2);
                doc.SurfaceHalf = CreateSKSurface(doc.BitmapHalf);
            }

            if (doc.Tracker.Document.Size.X > 1024 && doc.Tracker.Document.Size.Y > 1024)
            {
                doc.BitmapQuarter = CreateBitmap(doc.Tracker.Document.Size / 4);
                doc.SurfaceQuarter = CreateSKSurface(doc.BitmapQuarter);
            }

            if (doc.Tracker.Document.Size.X > 2048 && doc.Tracker.Document.Size.Y > 2048)
            {
                doc.BitmapEighth = CreateBitmap(doc.Tracker.Document.Size / 8);
                doc.SurfaceEighth = CreateSKSurface(doc.BitmapEighth);
            }

            doc.RaisePropertyChanged(nameof(doc.RenderBitmap));
        }

        private WriteableBitmap CreateBitmap(Vector2i size)
        {
            return new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Pbgra32, null);
        }

        private SKSurface CreateSKSurface(WriteableBitmap bitmap)
        {
            return SKSurface.Create(
                new SKImageInfo(bitmap.PixelWidth, bitmap.PixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()),
                bitmap.BackBuffer,
                bitmap.BackBufferStride);
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
                _ => throw new InvalidOperationException("Unsupposed member type")
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
